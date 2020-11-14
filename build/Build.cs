using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Logger;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;
using Nuke.Common.Tools.ReportGenerator;

[AzurePipelines(
    AzurePipelinesImage.WindowsLatest,
    InvokedTargets = new[] { nameof(Pack) },
    NonEntryTargets = new[] { nameof(Restore) },
    ExcludedTargets = new[] { nameof(Clean) },
    PullRequestsAutoCancel = true,
    PullRequestsBranchesInclude = new[] { "main" },
    TriggerBranchesInclude = new[] {
        "main",
        "feature/*",
        "fix/*"
    },
    TriggerPathsExclude = new[]
    {
        "docs/*",
        "README.md"
    }
)]
[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
public class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Pack);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

    [Parameter("Indicates wheter to restore nuget in interactive mode - Default is false")]
    public readonly bool Interactive = false;

    [Solution] public readonly Solution Solution;
    [GitRepository] public readonly GitRepository GitRepository;
    [GitVersion] public readonly GitVersion GitVersion;

    [CI] public readonly AzurePipelines AzurePipelines;

    [Partition(10)] public readonly Partition TestPartition;

    public AbsolutePath SourceDirectory => RootDirectory / "src";
    public AbsolutePath TestDirectory => RootDirectory / "test";

    public AbsolutePath OutputDirectory => RootDirectory / "output";

    public AbsolutePath CoverageReportDirectory => OutputDirectory / "coverage-report";

    public AbsolutePath TestResultDirectory => OutputDirectory / "tests-results";

    public AbsolutePath CompileOutputDirectory => OutputDirectory / "build";

    public AbsolutePath ArtifactsDirectory => OutputDirectory / "artifacts";

    public AbsolutePath BinDirectory => SourceDirectory / "bin";

    public Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
            EnsureCleanDirectory(TestResultDirectory);
            EnsureCleanDirectory(CoverageReportDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    public Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution)
                .SetIgnoreFailedSources(true)
                .SetDisableParallel(false)
                .When(IsLocalBuild && Interactive, _ => _.SetProperty("NugetInteractive", IsLocalBuild && Interactive))
            );
        });

    public Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetNoRestore(InvokedTargets.Contains(Restore))
                .SetConfiguration(Configuration)
                .SetProjectFile(Solution)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                );
        });

    public Target UnitTests => _ => _
        .DependsOn(Compile)
        .Description("Run unit tests and collect code")
        .Produces(TestResultDirectory / "*-unit-test.*.trx")
        .Produces(TestResultDirectory / "*-unit-test.*.xml")
        .Executes(() =>
        {
            Info("Start executing unit tests");
            IEnumerable<Project> projects = Solution.GetProjects("*.UnitTests");
            IEnumerable<Project> testsProjects = TestPartition.GetCurrent(projects);

            testsProjects.ForEach(project => Info(project));

            DotNetTest(s => s
                            .SetConfiguration(Configuration)
                            .ResetVerbosity()
                            .EnableCollectCoverage()
                            .SetNoBuild(InvokedTargets.Contains(Compile))
                            .SetResultsDirectory(TestResultDirectory)
                            .When(IsServerBuild, _ => _
                                .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                                .AddProperty("ExcludeByAttribute", "Obsolete")
                                .EnableUseSourceLink()
                            )
                            .CombineWith(testsProjects, (cs, project) => cs.SetProjectFile(project)
                            .CombineWith(project.GetTargetFrameworks(), (setting, framework) => setting
                                .SetFramework(framework)
                                .SetLogger($"trx;LogFileName={ TestResultDirectory / $"{project.Name}-unit-test.{framework}.trx"}")
                                .When(InvokedTargets.Contains(Coverage) || IsServerBuild, _ => _
                                    .SetCoverletOutput(TestResultDirectory / $"{project.Name}-unit-test.{framework}.xml")))));

                TestResultDirectory.GlobFiles("*-unit-test.*.trx").ForEach(testFileResult =>
                    AzurePipelines?.PublishTestResults(type: AzurePipelinesTestResultsType.VSTest,
                                                        title: $"{Path.GetFileNameWithoutExtension(testFileResult)} ({AzurePipelines.StageDisplayName})",
                                                        files: new string[] { testFileResult })); 
            
        });

    public Target Tests => _ => _
        .DependsOn(UnitTests);

    public Target Coverage => _ => _
        .DependsOn(Tests)
        .Consumes(Tests)
        .Produces(CoverageReportDirectory)
        .Executes(() =>
        {
            ReportGenerator(_ => _
                .SetReports(TestResultDirectory / "*.xml")
                .SetReportTypes(ReportTypes.HtmlInline_AzurePipelines)
                .SetTargetDirectory(CoverageReportDirectory));

            TestResultDirectory.GlobFiles("*.xml").ForEach(x =>
                AzurePipelines?.PublishCodeCoverage(
                    AzurePipelinesCodeCoverageToolType.Cobertura,
                    x,
                    CoverageReportDirectory));
        });

    public Target Pack => _ => _
        .DependsOn(Tests, Compile)
        .Consumes(Compile)
        .Produces(ArtifactsDirectory / "*.nupkg")
        .Executes(() =>
        {
            DotNetPack(s => s
                .EnableIncludeSource()
                .EnableIncludeSymbols()
                .SetOutputDirectory(ArtifactsDirectory)
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)

            );
        });


    protected override void OnTargetStart(string target)
    {
        Info($"Starting '{target}' task");
    }

    protected override void OnTargetExecuted(string target)
    {
        Info($"'{target}' task finished");
    }

    protected override void OnBuildInitialized()
    {
        Info($"{nameof(BuildProjectDirectory)} : {BuildProjectDirectory}");
        Info($"{nameof(GitVersion)} : {GitVersion.Jsonify()}");
    }
}
