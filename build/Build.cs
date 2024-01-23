// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

namespace Utilities.ContinuousIntegration;

using Candoumbe.Pipelines.Components;
using Candoumbe.Pipelines.Components.GitHub;
using Candoumbe.Pipelines.Components.NuGet;
using Candoumbe.Pipelines.Components.Workflows;

using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

using System;
using System.Collections.Generic;
using System.Linq;

using static Nuke.Common.Tools.Git.GitTasks;

[GitHubActions(
    "integration",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranchesIgnore = [IHaveMainBranch.MainBranchName],
    FetchDepth = 0,
    PublishArtifacts = true,
    EnableGitHubToken = true,
    InvokedTargets = [nameof(IUnitTest.UnitTests), nameof(IPack.Pack)],
    CacheKeyFiles = ["global.json", "src/**/*.csproj"],
    ImportSecrets =
    [
        nameof(NugetApiKey),
        nameof(IReportCoverage.CodecovToken)
    ],
    OnPullRequestExcludePaths =
    [
        "docs/*",
        "README.md",
        "CHANGELOG.md",
        "LICENSE"
    ]
)]
[GitHubActions(
    "delivery",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = [IHaveMainBranch.MainBranchName, IGitFlow.ReleaseBranch + "/*"],
    InvokedTargets = [nameof(IUnitTest.UnitTests), nameof(IPushNugetPackages.Publish), nameof(ICreateGithubRelease.AddGithubRelease)],
    EnableGitHubToken = true,
    FetchDepth = 0,
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" },
    PublishArtifacts = true,
    ImportSecrets =
    [
        nameof(NugetApiKey),
        nameof(IReportCoverage.CodecovToken)
    ],
    OnPullRequestExcludePaths =
    [
        "docs/*",
        "README.md",
        "CHANGELOG.md",
        "LICENSE"
    ]
)]

[UnsetVisualStudioEnvironmentVariables]
[DotNetVerbosityMapping]
public class Build : NukeBuild,
    IHaveArtifacts,
    IHaveConfiguration,
    IHaveSolution,
    IHaveChangeLog,
    IHaveSourceDirectory,
    IHaveTestDirectory,
    IHaveGitRepository,
    IHaveGitVersion,
    IClean,
    IRestore,
    IDotnetFormat,
    ICompile,
    IBenchmark,
    IUnitTest,
    IMutationTest,
    IReportCoverage,
    IPack,
    IPushNugetPackages,
    IGitFlowWithPullRequest,
    ICreateGithubRelease
{
    public static int Main() => Execute<Build>(x => ((ICompile)x).Compile);

    [Required]
    [Solution]
    public readonly Solution Solution;

    ///<inheritdoc/>
    Solution IHaveSolution.Solution => Solution;

    /// <summary>
    /// Token to interact with Nuget's API
    /// </summary>
    [Parameter("Token to interact with Nuget's API")]
    [Secret]
    public readonly string NugetApiKey;

    [CI] public readonly GitHubActions GitHubActions;

    ///<inheritdoc/>
    IEnumerable<AbsolutePath> IClean.DirectoriesToDelete => this.Get<IHaveSourceDirectory>().SourceDirectory.GlobDirectories("**/bin", "**/obj")
                                                            .Concat(this.Get<IHaveTestDirectory>().TestDirectory.GlobDirectories("**/bin", "**/obj"));

    ///<inheritdoc/>
    IEnumerable<AbsolutePath> IClean.DirectoriesToClean => new[] { this.Get<IPack>().ArtifactsDirectory, this.Get<IReportCoverage>().CoverageReportDirectory };

    ///<inheritdoc/>
    IEnumerable<AbsolutePath> IClean.DirectoriesToEnsureExistance => new[]
    {
        this.Get<IReportCoverage>().CoverageReportHistoryDirectory,
        this.Get<IMutationTest>().MutationTestResultDirectory,

    };

    ///<inheritdoc/>
    IEnumerable<Project> IUnitTest.UnitTestsProjects => Partition.GetCurrent(Solution.GetAllProjects("*.UnitTests"));

    ///<inheritdoc/>
    IEnumerable<MutationProjectConfiguration> IMutationTest.MutationTestsProjects
        => [new MutationProjectConfiguration(Solution.GetProject("Candoumbe.MiscUtilities"), Partition.GetCurrent(this.Get<IUnitTest>().UnitTestsProjects))];

    ///<inheritdoc/>
    IEnumerable<Project> IBenchmark.BenchmarkProjects => Solution.GetAllProjects("*.PerformanceTests");

    ///<inheritdoc/>
    Configure<DotNetRunSettings> IBenchmark.BenchmarksSettings => settings => settings.SetConfiguration(Configuration.Release);

    ///<inheritdoc/>
    IEnumerable<AbsolutePath> IPack.PackableProjects => this.Get<IHaveSourceDirectory>().SourceDirectory.GlobFiles("**/*.csproj");

    ///<inheritdoc/>
    IEnumerable<PushNugetPackageConfiguration> IPushNugetPackages.PublishConfigurations => new PushNugetPackageConfiguration[]
    {
        new NugetPushConfiguration(
            apiKey: NugetApiKey,
            source: new Uri("https://api.nuget.org/v3/index.json"),
            canBeUsed: () => NugetApiKey is not null
        ),
        new GitHubPushNugetConfiguration(
            githubToken: this.Get<ICreateGithubRelease>()?.GitHubToken,
            source: new Uri($"https://nuget.pkg.github.com/{GitHubActions?.RepositoryOwner}/index.json"),
            canBeUsed: () => this is ICreateGithubRelease createRelease && createRelease.GitHubToken is not null
    )};

    public Target Tests => _ => _
        .Description("Run all tests")
        .DependsOn(this.Get<IUnitTest>().UnitTests, this.Get<IMutationTest>().MutationTests);

    ///<inheritdoc/>
    bool IReportCoverage.ReportToCodeCov => this.Get<IReportCoverage>().CodecovToken is not null;

    ///<inheritdoc/>
    bool IDotnetFormat.VerifyNoChanges => IsServerBuild;

    ///<inheritdoc/>
    (IReadOnlyCollection<AbsolutePath> IncludedFiles, IReadOnlyCollection<AbsolutePath> ExcludedFiles) IDotnetFormat.Files
    {
        get
        {
            IReadOnlyCollection<AbsolutePath> includedFiles = Git("diff --name-status", workingDirectory: Solution.Directory)
                        .Where(output => output.Text.AsSpan().StartsWith("M") || output.Text.AsSpan().StartsWith("A"))
                        .Select(output => AbsolutePath.Create(output.Text.AsSpan()[1..].TrimStart().ToString()))
                        .ToArray();

            IReadOnlyCollection<AbsolutePath> excludedFiles = [];

            return (includedFiles, excludedFiles);
        }
    }

    ///<inheritdoc/>
    protected override void OnBuildCreated()
    {
        if (IsServerBuild)
        {
            EnvironmentInfo.SetVariable("DOTNET_ROLL_FORWARD", "LatestMajor");
        }
    }
}
