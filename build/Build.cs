using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Logger;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.GitReleaseManager.GitReleaseManagerTasks;
using static Nuke.Common.Tools.GitVersion.GitVersionTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

using System;

namespace Utilities.Pipelines
{
    [GitHubActions(
        "continuous",
        GitHubActionsImage.WindowsLatest,
        OnPushBranchesIgnore = new[] { MainBranchName, ReleaseBranchPrefix + "/*" },
        OnPullRequestBranches = new[] { DevelopBranch },
        PublishArtifacts = true,
        InvokedTargets = new[] { nameof(Tests), nameof(Pack) },
        OnPullRequestExcludePaths = new[] {
        "docs/*",
        "README.md",
        "CHANGELOG.md",
        "LICENSE"
        }
    )]
    [GitHubActions(
        "deployment",
        GitHubActionsImage.WindowsLatest,
        OnPushBranches = new[] { MainBranchName, ReleaseBranchPrefix + "/*" },
        InvokedTargets = new[] { nameof(Publish) },
        ImportGitHubTokenAs = nameof(GitHubToken),
        PublishArtifacts = true,
        ImportSecrets = new[] { nameof(NugetApiKey) },
        OnPullRequestExcludePaths = new[] {
        "docs/*",
        "README.md",
        "CHANGELOG.md",
        "LICENSE"
        }
    )]
    [AzurePipelines(
        suffix: "pull-request",
        AzurePipelinesImage.WindowsLatest,
        InvokedTargets = new[] { nameof(Tests) },
        NonEntryTargets = new[] { nameof(Restore) },
        ExcludedTargets = new[] { nameof(Clean) },
        TriggerBranchesInclude = new[]
        {
            FeatureBranchPrefix + "/*",
            SupportBranchPrefix + "/*",
            HotfixBranchPrefix + "/*"
        },
        TriggerPathsExclude = new[]
        {
            "docs/*",
            "README.md"
        }
    )]
    [AzurePipelines(
        AzurePipelinesImage.WindowsLatest,
        InvokedTargets = new[] { nameof(Pack) },
        NonEntryTargets = new[] { nameof(Restore) },
        ExcludedTargets = new[] { nameof(Clean) },
        PullRequestsAutoCancel = true,
        TriggerBranchesInclude = new[]
        {
            MainBranchName
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
        public static int Main() => Execute<Build>(x => x.Compile);

        [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
        public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

        [Parameter("Indicates wheter to restore nuget in interactive mode - Default is false")]
        public readonly bool Interactive = false;

        [Parameter("Indicates to automatically stash before switching branch")]
        public readonly bool AutoStash = true;

        [Solution] public readonly Solution Solution;
        [GitRepository] public readonly GitRepository GitRepository;

        [GitVersion] public readonly GitVersion GitVersion;

        [CI] public readonly AzurePipelines AzurePipelines;

        [CI] public readonly GitHubActions GitHubActions;

        [Parameter("Token used to interact with Github")]
        public readonly string GitHubToken;

        [Partition(10)] public readonly Partition TestPartition;

        public AbsolutePath SourceDirectory => RootDirectory / "src";

        public AbsolutePath TestDirectory => RootDirectory / "test";

        public AbsolutePath OutputDirectory => RootDirectory / "output";

        public AbsolutePath CoverageReportDirectory => OutputDirectory / "coverage-report";

        public AbsolutePath CoverageHistoryDirectory => OutputDirectory / "coverage-history";

        public AbsolutePath TestResultDirectory => OutputDirectory / "tests-results";

        public AbsolutePath ArtifactsDirectory => OutputDirectory / "artifacts";

        public const string MainBranchName = "main";
        public const string DevelopBranch = "develop";
        public const string FeatureBranchPrefix = "feature";
        public const string HotfixBranchPrefix = "hotfix";
        public const string ReleaseBranchPrefix = "release";
        public const string SupportBranchPrefix = "support";

        public Target Clean => _ => _
            .Description($"Cleans all 'bin' and 'obj' directories under '{SourceDirectory}' and '{TestDirectory}'")
            .Before(Restore)
            .Executes(() =>
            {
                SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                EnsureCleanDirectory(TestResultDirectory);
                EnsureCleanDirectory(ArtifactsDirectory);
                EnsureCleanDirectory(CoverageReportDirectory);
            });

        public Target Restore => _ => _
            .Description("Restores nuget packages dependencies")
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
                    .SetInformationalVersion(GitVersion.InformationalVersion));
            });

        public Target Tests => _ => _
            .DependsOn(Compile)
            .Description("Run unit tests and collect code coverage results.")
            .Produces(TestResultDirectory / "*.trx")
            .Produces(TestResultDirectory / "*.xml")
            .Executes(() =>
            {
                IEnumerable<Project> projects = Solution.GetProjects("*.UnitTests");
                IEnumerable<Project> testsProjects = TestPartition.GetCurrent(projects);

                testsProjects.ForEach(project => Info(project));

                DotNetTest(s => s
                    .SetConfiguration(Configuration)
                    .EnableCollectCoverage()
                    .EnableUseSourceLink()
                    .SetNoBuild(InvokedTargets.Contains(Compile))
                    .SetResultsDirectory(TestResultDirectory)
                    .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                    .AddProperty("ExcludeByAttribute", "Obsolete")
                    .CombineWith(testsProjects, (cs, project) => cs.SetProjectFile(project)
                        .CombineWith(project.GetTargetFrameworks(), (setting, framework) => setting
                            .SetFramework(framework)
                            .SetLogger($"trx;LogFileName={project.Name}.{framework}.trx")
                            .SetCoverletOutput(TestResultDirectory / $"{project.Name}.xml"))
                        )
                );

                TestResultDirectory.GlobFiles("*.trx")
                                   .ForEach(testFileResult => AzurePipelines?.PublishTestResults(type: AzurePipelinesTestResultsType.VSTest,
                                                                                                 title: $"{Path.GetFileNameWithoutExtension(testFileResult)} ({AzurePipelines.StageDisplayName})",
                                                                                                 files: new string[] { testFileResult })
                );

                // TODO Move this to a separate "coverage" target once https://github.com/nuke-build/nuke/issues/562 is solved !
                ReportGenerator(_ => _
                        .SetFramework("net5.0")
                        .SetReports(TestResultDirectory / "*.xml")
                        .SetReportTypes(ReportTypes.Badges, ReportTypes.HtmlChart, ReportTypes.HtmlInline_AzurePipelines_Dark)
                        .SetTargetDirectory(CoverageReportDirectory)
                        .SetHistoryDirectory(CoverageHistoryDirectory)
                        .SetTag(MajorMinorPatchVersion)
                    );

                TestResultDirectory.GlobFiles("*.xml")
                                   .ForEach(file => AzurePipelines?.PublishCodeCoverage(coverageTool: AzurePipelinesCodeCoverageToolType.Cobertura,
                                                                                        summaryFile: file,
                                                                                        reportDirectory: CoverageReportDirectory));
            });

        public Target Pack => _ => _
            .DependsOn(Tests, Compile)
            .Consumes(Compile)
            .Produces(ArtifactsDirectory / "*.nupkg",
                      ArtifactsDirectory / "*.snupkg")
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
                    .SetVersion(GitVersion.NuGetVersion)
                    .SetPackageReleaseNotes(GetNuGetReleaseNotes(ChangeLogFile, GitRepository))
                    .SetSymbolPackageFormat(DotNetSymbolPackageFormat.snupkg)
                );
            });


        #region Git flow section

        private AbsolutePath ChangeLogFile => RootDirectory / "CHANGELOG.md";

        public Target Changelog => _ => _
            .OnlyWhenStatic(() => GitRepository.IsOnReleaseBranch() || GitRepository.IsOnHotfixBranch())
            .Description("Finalizes the change log so that its up to date for the release. ")
            .Executes(() =>
            {
                FinalizeChangelog(ChangeLogFile, GitVersion.MajorMinorPatch, GitRepository);
                Info($"Please review CHANGELOG.md ({ChangeLogFile}) and press 'Y' to validate (any other key will cancel changes)...");
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.Y)
                {
                    Git($"add {ChangeLogFile}");
                    Git($"commit -m \"Finalize {Path.GetFileName(ChangeLogFile)} for {GitVersion.MajorMinorPatch}\"");
                }
            });

        public Target Feature => _ => _
        .Description($"Starts a new feature development by creating the associated branch {FeatureBranchPrefix}/{{feature-name}} from {DevelopBranch}")
        .Requires(() => IsLocalBuild)
        .Requires(() => !GitRepository.IsOnFeatureBranch() || GitHasCleanWorkingCopy())
        .Executes(() =>
        {
            if (!GitRepository.IsOnFeatureBranch())
            {
                Info("Enter the name of the feature. It will be used as the name of the feature/branch (leave empty to exit) :");
                string featureName;
                bool exitCreatingFeature = false;
                do
                {
                    featureName = (Console.ReadLine() ?? string.Empty).Trim()
                                                                    .Trim('/');

                    switch (featureName)
                    {
                        case string name when !string.IsNullOrWhiteSpace(name):
                            {
                                string branchName = $"{FeatureBranchPrefix}/{featureName.Slugify()}";
                                Info($"{Environment.NewLine}The branch '{branchName}' will be created.{Environment.NewLine}Confirm ? (Y/N) ");

                                switch (Console.ReadKey().Key)
                                {
                                    case ConsoleKey.Y:
                                        Info($"{Environment.NewLine}Checking out branch '{branchName}' from '{DevelopBranch}'");
                                        Checkout(branchName, start: DevelopBranch);
                                        Info($"{Environment.NewLine}'{branchName}' created successfully");
                                        exitCreatingFeature = true;
                                        break;

                                    default:
                                        Info($"{Environment.NewLine}Exiting {nameof(Feature)} task.");
                                        exitCreatingFeature = true;
                                        break;
                                }
                            }
                            break;
                        default:
                            Info($"Exiting {nameof(Feature)} task.");
                            exitCreatingFeature = true;
                            break;
                    }

                } while (string.IsNullOrWhiteSpace(featureName) && !exitCreatingFeature);

                Info($"{EnvironmentInfo.NewLine}Good bye !");
            }
            else
            {
                FinishFeature();
            }
        });

        string MajorMinorPatchVersion => GitVersion.MajorMinorPatch;
        public Target Release => _ => _
            .DependsOn(Changelog)
            .Description($"Starts a new {ReleaseBranchPrefix}/{{version}} from {DevelopBranch}")
            .Requires(() => !GitRepository.IsOnReleaseBranch() || GitHasCleanWorkingCopy())
            .Executes(() =>
            {
                if (!GitRepository.IsOnReleaseBranch())
                {
                    Checkout($"{ReleaseBranchPrefix}/{MajorMinorPatchVersion}", start: DevelopBranch);
                }
                else
                {
                    FinishReleaseOrHotfix();
                }
            });


        public Target Hotfix => _ => _
            .DependsOn(Changelog)
            .Description($"Starts a new hotfix branch '{HotfixBranchPrefix}/*' from {MainBranchName}")
            .Requires(() => !GitRepository.IsOnHotfixBranch() || GitHasCleanWorkingCopy())
            .Executes(() =>
            {
                (GitVersion mainBranchVersion, IReadOnlyCollection<Output> _) = GitVersion(s => s.SetFramework("net5.0")
                                                                                                 .SetUrl(RootDirectory)
                                                                                                 .SetBranch(MainBranchName)
                                                                                                 .EnableNoFetch()
                                                                                                 .DisableProcessLogOutput());

                if (!GitRepository.IsOnHotfixBranch())
                {
                    Checkout($"{HotfixBranchPrefix}/{mainBranchVersion.Major}.{mainBranchVersion.Minor}.{mainBranchVersion.Patch + 1}", start: MainBranchName);
                }
                else
                {
                    FinishReleaseOrHotfix();
                }
            });

        private void Checkout(string branch, string start)
        {
            bool hasCleanWorkingCopy = GitHasCleanWorkingCopy();

            if (!hasCleanWorkingCopy && AutoStash)
            {
                Git("stash");
            }
            Git($"checkout -b {branch} {start}");

            if (!hasCleanWorkingCopy && AutoStash)
            {
                Git("stash apply");
            }
        }


        private void FinishReleaseOrHotfix()
        {
            Git($"checkout {MainBranchName}");
            Git($"merge --no-ff --no-edit {GitRepository.Branch}");
            Git($"tag {MajorMinorPatchVersion}");

            Git($"checkout {DevelopBranch}");
            Git($"merge --no-ff --no-edit {GitRepository.Branch}");

            Git($"branch -D {GitRepository.Branch}");

            Git($"push origin --follow-tags {MainBranchName} {DevelopBranch} {MajorMinorPatchVersion}");
        }

        private void FinishFeature()
        {
            Git($"rebase {DevelopBranch}");
            Git($"checkout {DevelopBranch}");
            Git($"merge --no-ff --no-edit {GitRepository.Branch}");

            Git($"branch -D {GitRepository.Branch}");
            Git($"push origin {DevelopBranch}");
        }

        #endregion

        [Parameter("API key used to publish artifacts to Nuget.org")]
        public readonly string NugetApiKey;

        [Parameter(@"URI where packages should be published (default : ""https://api.nuget.org/v3/index.json""")]
        public string NugetPackageSource => "https://api.nuget.org/v3/index.json";

        public string GitHubPackageSource => $"https://nuget.pkg.github.com/{GitHubActions.GitHubRepositoryOwner}/index.json";

        public bool IsOnGithub => GitHubActions is not null;

        public Target Publish => _ => _
            .Description($"Published packages (*.nupkg and *.snupkg) to the destination server set with {nameof(NugetPackageSource)} settings ")
            .DependsOn(Clean, Tests, Pack)
            .Consumes(Pack, ArtifactsDirectory / "*.nupkg", ArtifactsDirectory / "*.snupkg")
            .Requires(() => !NugetApiKey.IsNullOrEmpty())
            .Requires(() => GitHasCleanWorkingCopy())
            .Requires(() => GitRepository.Branch == MainBranchName
                            || GitRepository.IsOnReleaseBranch()
                            || GitRepository.IsOnDevelopBranch())
            .Requires(() => Configuration.Equals(Configuration.Release))
            .Executes(() =>
            {
                void PushPackages(IReadOnlyCollection<AbsolutePath> nupkgs)
                {
                    Info($"Publishing {nupkgs.Count} package{(nupkgs.Count > 1 ? "s" : string.Empty)}");
                    Info(string.Join(EnvironmentInfo.NewLine, nupkgs));

                    DotNetNuGetPush(s => s.SetApiKey(NugetApiKey)
                        .SetSource(NugetPackageSource)
                        .EnableSkipDuplicate()
                        .EnableNoSymbols()
                        .SetProcessLogTimestamp(true)
                        .CombineWith(nupkgs, (_, nupkg) => _
                                    .SetTargetPath(nupkg)),
                        degreeOfParallelism: 4,
                        completeOnFailure: true);
                }

                if (IsOnGithub)
                {
                    DotNetNuGetAddSource(_ => _
                        .SetSource(GitHubPackageSource)
                        .SetUsername(GitHubActions.GitHubActor)
                        .SetPassword(GitHubToken));
                }
                PushPackages(ArtifactsDirectory.GlobFiles("*.nupkg"));
                PushPackages(ArtifactsDirectory.GlobFiles("*.snupkg"));
            });



    }
}