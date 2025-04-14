// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using Nuke.Common.Tools.GitHub;

namespace Utilities.ContinuousIntegration;

using System;
using System.Collections.Generic;
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

[GitHubActions(
    "integration",
    GitHubActionsImage.Ubuntu2204,
    OnPushBranchesIgnore = [IHaveMainBranch.MainBranchName],
    AutoGenerate = false,
    FetchDepth = 0,
    PublishArtifacts = true,
    EnableGitHubToken = true,
    InvokedTargets = [nameof(IUnitTest.UnitTests), nameof(IMutationTest.MutationTests), nameof(IPack.Pack)],
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
    "nightly",
    GitHubActionsImage.Ubuntu2204,
    OnCronSchedule = "0 0 * * *",
    OnPushBranches = [IHaveDevelopBranch.DevelopBranchName],
    AutoGenerate = false,
    FetchDepth = 0,
    PublishArtifacts = true,
    EnableGitHubToken = true,
    InvokedTargets = [nameof(IUnitTest.UnitTests), nameof(IMutationTest.MutationTests), nameof(IPack.Pack)],
    CacheKeyFiles = [
        "global.json",
        "src/**/*.csproj",
        "src/**/*.csproj",
        "test/**/stryker-config.json",
        "test/**/xunit.runner.json"
    ],
    ImportSecrets =
    [
        nameof(NugetApiKey),
        nameof(IReportCoverage.CodecovToken),
        nameof(IMutationTest.StrykerDashboardApiKey)
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
    GitHubActionsImage.Ubuntu2204,
    AutoGenerate = false,
    OnPushBranches = [IHaveMainBranch.MainBranchName, IGitFlow.ReleaseBranch + "/*"],
    InvokedTargets = [nameof(IUnitTest.UnitTests), nameof(IPushNugetPackages.Publish), nameof(ICreateGithubRelease.AddGithubRelease)],
    EnableGitHubToken = true,
    FetchDepth = 0,
    CacheKeyFiles = ["global.json", "src/**/*.csproj"],
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
public class Build : EnhancedNukeBuild,
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
    IReportUnitTestCoverage,
    IPack,
    IPushNugetPackages,
    IGitFlowWithPullRequest,
    ICreateGithubRelease
{
    public static int Main() => Execute<Build>(x => ((ICompile)x).Compile);

    [Required] [Solution] public readonly Solution Solution;

    ///<inheritdoc/>
    Solution IHaveSolution.Solution => Solution;

    /// <summary>
    /// Token to interact with Nuget's API
    /// </summary>
    [Parameter("Token to interact with Nuget's API")] [Secret]
    public readonly string NugetApiKey;

    [CI] public readonly GitHubActions GitHubActions;

    ///<inheritdoc/>
    IEnumerable<AbsolutePath> IClean.DirectoriesToDelete =>
        [ 
            ..this.Get<IHaveSourceDirectory>().SourceDirectory.GlobDirectories("**/bin", "**/obj"),
            ..this.Get<IHaveTestDirectory>().TestDirectory.GlobDirectories("**/bin", "**/obj")
        ];

    ///<inheritdoc/>
    IEnumerable<AbsolutePath> IClean.DirectoriesToClean =>
    [
        this.Get<IPack>().ArtifactsDirectory,
        this.Get<IReportCoverage>().CoverageReportDirectory
    ];

    ///<inheritdoc/>
    IEnumerable<AbsolutePath> IClean.DirectoriesToEnsureExistence
        =>
        [
            this.Get<IReportCoverage>().CoverageReportHistoryDirectory,
            this.Get<IMutationTest>().MutationTestResultDirectory
        ];

    ///<inheritdoc/>
    IEnumerable<Project> IUnitTest.UnitTestsProjects => Partition.GetCurrent(Solution.GetAllProjects("*.UnitTests"));

    ///<inheritdoc/>
    IEnumerable<MutationProjectConfiguration> IMutationTest.MutationTestsProjects => [new (Solution.GetProject("Candoumbe.MiscUtilities"), this.Get<IUnitTest>().UnitTestsProjects) ];

    ///<inheritdoc/>
    IEnumerable<Project> IBenchmark.BenchmarkProjects => Solution.GetAllProjects("*.PerformanceTests");

    ///<inheritdoc/>
    Configure<DotNetRunSettings> IBenchmark.BenchmarksSettings => settings => settings.SetConfiguration(Configuration.Release);

    ///<inheritdoc/>
    IEnumerable<AbsolutePath> IPack.PackableProjects => this.Get<IHaveSourceDirectory>().SourceDirectory.GlobFiles("**/*.csproj");

    ///<inheritdoc/>
    IEnumerable<PushNugetPackageConfiguration> IPushNugetPackages.PublishConfigurations =>
    [
        new NugetPushConfiguration(
            apiKey: NugetApiKey,
            source: new Uri("https://api.nuget.org/v3/index.json"),
            canBeUsed: () => NugetApiKey is not null
        ),
        new GitHubPushNugetConfiguration(
            githubToken: this.Get<ICreateGithubRelease>()?.GitHubToken,
            source: new Uri($"https://nuget.pkg.github.com/{this.Get<IHaveGitHubRepository>().GitRepository.GetGitHubOwner()}/index.json"),
            canBeUsed: () => this is ICreateGithubRelease { GitHubToken: not null } createRelease
        )
    ];

    public Target Tests => _ => _
        .Description("Run all tests")
        .DependsOn(this.Get<IUnitTest>().UnitTests, this.Get<IMutationTest>().MutationTests);

    ///<inheritdoc/>
    bool IReportCoverage.ReportToCodeCov => this.Get<IReportCoverage>().CodecovToken is not null;
}