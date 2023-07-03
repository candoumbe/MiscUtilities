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
using Nuke.Common.Tools.DotNet;

using System;
using System.Collections.Generic;
using System.Linq;

[GitHubActions(
    "integration",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranchesIgnore = new[] { IHaveMainBranch.MainBranchName },
    FetchDepth = 0,
    PublishArtifacts = true,
    EnableGitHubToken = true,
    InvokedTargets = new[] { nameof(IUnitTest.UnitTests), nameof(IPack.Pack) },
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" },
    ImportSecrets = new[]
    {
            nameof(NugetApiKey),
            nameof(IReportCoverage.CodecovToken)
    },
    OnPullRequestExcludePaths = new[]
    {
            "docs/*",
            "README.md",
            "CHANGELOG.md",
            "LICENSE"
    }
)]
[GitHubActions(
    "delivery",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] { IHaveMainBranch.MainBranchName, IGitFlow.ReleaseBranch + "/*" },
    InvokedTargets = new[] { nameof(IUnitTest.UnitTests), nameof(IPushNugetPackages.Publish), nameof(ICreateGithubRelease.AddGithubRelease) },
    EnableGitHubToken = true,
    FetchDepth = 0,
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" },
    PublishArtifacts = true,
    ImportSecrets = new[]
    {
            nameof(NugetApiKey),
            nameof(IReportCoverage.CodecovToken)
    },
    OnPullRequestExcludePaths = new[]
    {
            "docs/*",
            "README.md",
            "CHANGELOG.md",
            "LICENSE"
    }
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
    IEnumerable<(Project SourceProject, IEnumerable<Project> TestProjects)> IMutationTest.MutationTestsProjects
        => new[] { (Solution.GetProject("Candoumbe.MiscUtilities"), Partition.GetCurrent(this.Get<IUnitTest>().UnitTestsProjects)) };

    ///<inheritdoc/>
    IEnumerable<Project> IBenchmark.BenchmarkProjects => Solution.GetAllProjects("*.PerformanceTests");

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
    protected override void OnBuildCreated()
    {
        if (IsServerBuild)
        {
            EnvironmentInfo.SetVariable("DOTNET_ROLL_FORWARD", "LatestMajor");
        }
    }
}
