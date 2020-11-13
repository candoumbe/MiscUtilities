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

using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Logger;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[AzurePipelines(
    AzurePipelinesImage.UbuntuLatest,
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

    [CI] public readonly AzurePipelines AzurePipelines;

    [Partition(10)] public readonly Partition TestPartition;

    public AbsolutePath SourceDirectory => RootDirectory / "src";
    public AbsolutePath TestDirectory => RootDirectory / "test";

    public AbsolutePath OutputDirectory => RootDirectory / "output";

    public AbsolutePath CoverageReportDirectory => OutputDirectory / "coverage-report";

    public AbsolutePath TestResultDirectory => OutputDirectory / "tests-results";

    public Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
            EnsureCleanDirectory(TestResultDirectory);
            EnsureCleanDirectory(CoverageReportDirectory);
        });

    public Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            
            DotNetRestore(s => s
                .SetProjectFile(Solution)
                .SetIgnoreFailedSources(true)
                .When(IsLocalBuild && Interactive, _ => _.SetProperty("NugetInteractive", IsLocalBuild && Interactive))
            );
        });

    public Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetConfiguration(Configuration)
                .SetProjectFile(Solution)
                .SetNoRestore(InvokedTargets.Contains(Restore))
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

            if (testsProjects.Any())
            {
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
                    AzurePipelines?.PublishTestResults(type: AzurePipelinesTestResultsType.XUnit,
                                                       title: $"{Path.GetFileNameWithoutExtension(testFileResult)} ({AzurePipelines.StageDisplayName})",
                                                       files: new string[] { testFileResult })); 
            }
        });

    public Target IntegrationTests => _ => _
        .DependsOn(Compile)
        .Description("Run integration tests and collect code coverage")
        .Produces(TestResultDirectory / "*-integration.*.trx")
        .Produces(TestResultDirectory / "*-integration.*.xml")
        .Executes(() =>
        {
            IEnumerable<Project> projects = Solution.GetProjects("*.IntegrationTests");
            IEnumerable<Project> testsProjects = TestPartition.GetCurrent(projects);

            testsProjects.ForEach(project => Info(project));
            
            if (testsProjects.Any())
            {
                DotNetTest(s => s
                        .SetConfiguration(Configuration)
                        .ResetVerbosity()
                        .EnableCollectCoverage()
                        .SetNoBuild(InvokedTargets.Contains(Compile))
                        .SetResultsDirectory(TestResultDirectory)
                        .When(IsServerBuild, _ => _.SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                                                   .AddProperty("ExcludeByAttribute", "Obsolete")
                                                   .EnableUseSourceLink()
                        )
                        .CombineWith(testsProjects, (cs, project) => cs.SetProjectFile(project)
                            .CombineWith(project.GetTargetFrameworks(), (setting, framework) => setting
                                .SetFramework(framework)
                                .SetLogger($"trx;LogFileName={ TestResultDirectory / $"{project.Name}-integration.{framework}.trx"}")
                                .When(InvokedTargets.Contains(Coverage) || IsServerBuild, _ => _
                                    .SetCoverletOutput(TestResultDirectory / $"{project.Name}-integration.{framework}.xml")))));

                TestResultDirectory.GlobFiles("-integration.*.trx").ForEach(testFileResult =>
                    AzurePipelines?.PublishTestResults(type: AzurePipelinesTestResultsType.XUnit,
                                                       title: $"{Path.GetFileNameWithoutExtension(testFileResult)} ({AzurePipelines.StageDisplayName})",
                                                       files: new string[] { testFileResult })); 
            }
        });

    public Target Tests => _ => _
        .DependsOn(UnitTests, IntegrationTests)
        .Produces(TestResultDirectory / "*.xml")
        .Produces(TestResultDirectory / "*.trx")
        .Executes(() =>
        {
        });

    public Target Coverage => _ => _
        .DependsOn(UnitTests, IntegrationTests)
        .Consumes(Tests)
        .Executes(() =>
        {
        });

    public Target Pack => _ => _
        .DependsOn(Tests)
        .Produces(OutputDirectory / "*.nupkg")
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetNoBuild(InvokedTargets.Contains(Compile))
                .SetNoRestore(InvokedTargets.Contains(Restore))
                .SetOutputDirectory(OutputDirectory)
                .EnableIncludeSymbols()
            );
        });

    public Target InstallSdks = _ => _
        .OnlyWhenStatic(() => IsServerBuild)
        .Executes(() =>
        {

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
    }
}
