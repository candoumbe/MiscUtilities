// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

namespace Utilities.ContinuousIntegration;

using Candoumbe.Pipelines.Components;

using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;

using System.Collections.Generic;
using System.Linq;

using static Nuke.Common.IO.PathConstruction;

[GitHubActions(
    "pull-request",
    GitHubActionsImage.UbuntuLatest,
    OnPullRequestBranches = new[] { IGitFlow.DevelopBranch },
    PublishArtifacts = true,
    FetchDepth = 0,
    EnableGitHubToken = true,
    InvokedTargets = new[] { nameof(IUnitTest.UnitTests), nameof(IReportCoverage.ReportCoverage) },
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" },
    ImportSecrets = new[]
    {
            nameof(CodecovToken),
            nameof(StrykerDashboardApiKey)
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
    "integration",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranchesIgnore = new[] { IGitFlow.MainBranchName },
    FetchDepth = 0,
    PublishArtifacts = true,
    EnableGitHubToken = true,
    InvokedTargets = new[] { nameof(IUnitTest.UnitTests), nameof(IReportCoverage.ReportCoverage), nameof(IPack.Pack) },
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" },
    ImportSecrets = new[]
    {
            nameof(NugetApiKey),
            nameof(CodecovToken),
            nameof(StrykerDashboardApiKey)
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
    OnPushBranches = new[] { IGitFlow.MainBranchName, IGitFlow.ReleaseBranch + "/*" },
    InvokedTargets = new[] { nameof(IUnitTest.UnitTests), nameof(IReportCoverage.ReportCoverage), nameof(IPublish.Publish), nameof(ICreateGithubRelease.AddGithubRelease) },
    EnableGitHubToken = true,
    FetchDepth = 0,
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" },
    PublishArtifacts = true,
    ImportSecrets = new[]
    {
            nameof(NugetApiKey),
            nameof(CodecovToken)
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
[HandleVisualStudioDebugging]
public class Build : NukeBuild,
    IHaveConfiguration,
    IHaveSolution,
    IHaveChangeLog,
    IHaveSourceDirectory,
    IHaveTestDirectory,
    IHaveGitRepository,
    IHaveGitVersion,
    IGitFlow,
    IClean,
    IRestore,
    ICompile,
    IBenchmark,
    IUnitTest,
    IReportCoverage,
    IMutationTest,
    IPack,
    IPublish,
    ICreateGithubRelease
{
    public static int Main() => Execute<Build>(x => ((ICompile)x).Compile);

    [Required]
    [Solution]
    public readonly Solution Solution;

    ///<inheritdoc/>
    Solution IHaveSolution.Solution => Solution;

    /// <summary>
    /// Token to interact with GitHub's API
    /// </summary>
    [Parameter]
    [Secret]
    public readonly string GitHubToken;

    /// <summary>
    /// Token to interact with Nuget's API
    /// </summary>
    [Parameter("Token to interact with Nuget's API")]
    [Secret]
    public readonly string NugetApiKey;

    [Parameter]
    [Secret]
    public readonly string CodecovToken;

    ///<inheritdoc/>
    string IReportCoverage.CodecovToken => CodecovToken;

    [Parameter("API Key used to submit Stryker dashboard")]
    [Secret]
    public readonly string StrykerDashboardApiKey;

    [GitVersion(NoFetch = true, Framework = "net5.0")]
    public readonly GitVersion GitVersion;

    [GitRepository]
    public readonly GitRepository GitRepository;

    ///<inheritdoc/>
    GitRepository IHaveGitRepository.GitRepository => GitRepository;

    ///<inheritdoc/>
    GitVersion IHaveGitVersion.GitVersion => GitVersion;

    [CI] public readonly GitHubActions GitHubActions;

    /// <summary>
    /// Directory where to store all output builds output
    /// </summary>
    public AbsolutePath OutputDirectory => RootDirectory / "output";

    /// <summary>
    /// Directory where to pu
    /// </summary>
    public AbsolutePath CoverageReportDirectory => OutputDirectory / "coverage-report";

    /// <summary>
    /// Directory where to publish all test results
    /// </summary>
    public AbsolutePath TestResultDirectory => OutputDirectory / "tests-results";

    /// <summary>
    /// Directory where to publish all artifacts
    /// </summary>
    public AbsolutePath ArtifactsDirectory => OutputDirectory / "artifacts";

    /// <summary>
    /// Directory where to publish converage history report
    /// </summary>
    public AbsolutePath CoverageReportHistoryDirectory => OutputDirectory / "coverage-history";


    /// <summary>
    /// Directory where to publish benchmark results.
    /// </summary>
    public AbsolutePath BenchmarkDirectory => OutputDirectory / "benchmarks";

    ///<inheritdoc/>
    IEnumerable<AbsolutePath> IClean.DirectoriesToDelete => From<IHaveSourceDirectory>().SourceDirectory.GlobDirectories("**/bin", "**/obj")
                                                            .Concat(From<IHaveTestDirectory>().TestDirectory.GlobDirectories("**/bin", "**/obj"));

    ///<inheritdoc/>
    IEnumerable<AbsolutePath> IClean.DirectoriesToClean => new[] { From<IPack>().ArtifactsDirectory, From<IReportCoverage>().CoverageReportDirectory };

    ///<inheritdoc/>
    IEnumerable<AbsolutePath> IClean.DirectoriesToEnsureExistance => new[]
    {
        From<IReportCoverage>().CoverageReportHistoryDirectory,
        From<IMutationTest>().MutationTestResultDirectory,

    };

    ///<inheritdoc/>
    IEnumerable<Project> IUnitTest.UnitTestsProjects => Partition.GetCurrent(From<IHaveSolution>().Solution.GetProjects("*.UnitTests"));

    ///<inheritdoc/>
    IEnumerable<Project> IMutationTest.MutationTestsProjects => Partition.GetCurrent(From<IUnitTest>().UnitTestsProjects);

    ///<inheritdoc/>
    IEnumerable<Project> IBenchmark.BenchmarkProjects => From<IHaveSolution>().Solution.GetProjects("*.PerformanceTests");

    ///<inheritdoc/>
    AbsolutePath IBenchmark.BenchmarkResultDirectory => From<IHaveArtifacts>().ArtifactsDirectory / "benchmarks";

    ///<inheritdoc/>
    IEnumerable<AbsolutePath> IPack.PackableProjects => From<IHaveSourceDirectory>().SourceDirectory.GlobFiles("**/*.csproj");

    ///<inheritdoc/>
    bool IReportCoverage.ReportToCodeCov => CodecovToken is not null;

    public Target Tests => _ => _
        .Description("Run all tests")
        .DependsOn(From<IUnitTest>().UnitTests, From<IMutationTest>().MutationTests);

    ///<inheritdoc/>
    protected override void OnBuildCreated()
    {
        if (IsServerBuild)
        {
            EnvironmentInfo.SetVariable("DOTNET_ROLL_FORWARD", "Major");
        }
    }

    private T From<T>() where T : INukeBuild => (T)(object)this;
}
