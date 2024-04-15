using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.ReportGenerator;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[GitHubActions(
    "continuous",
    GitHubActionsImage.UbuntuLatest,
    On = [GitHubActionsTrigger.Push],
    AutoGenerate = true,
    CacheKeyFiles = ["**/global.json", "**/*.csproj"],
    CacheIncludePatterns = [".nuke/temp", "~/.nuget/packages"],
    CacheExcludePatterns = [],
    EnableGitHubToken = true,
    FetchDepth = 0,
    PublishArtifacts = true,
    InvokedTargets = [nameof(TearDown)])]
public class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.TearDown);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution]
    readonly Solution Solution;
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath PublishDirectory => RootDirectory / "publish";
    List<Project> WebApiProjects => Solution.AllProjects.Where(p => p.Name.Contains("Api")).ToList();
    List<Project> TestProjects => Solution.AllProjects.Where(p => p.Name.Contains(".Tests")).ToList();
    AbsolutePath TestReportDirectory => ArtifactsDirectory / "test-reports";
    AbsolutePath CoverageReportDirectory => ArtifactsDirectory / "coverage-reports";
    AbsolutePath CoverageReport => CoverageReportDirectory / "coverage.xml";
    bool IsRunningAzurePipelines => false;
    AzurePipelines AzurePipelines => AzurePipelines.Instance;
    GitHubActions GitHubActions => GitHubActions.Instance;
    string Version { get; set; }

    Target Setup => _ => _
        .Executes(() =>
        {
            TestReportDirectory.CreateOrCleanDirectory();
            ArtifactsDirectory.CreateOrCleanDirectory();
            PublishDirectory.CreateOrCleanDirectory();
            CoverageReportDirectory.CreateOrCleanDirectory();

            TestProjects.ForEach(p => Log.Information("test project:{projectName}", p.Name));
        });

    Target ObtainVersion => _ => _
        .Before(Compile)
        .After(Setup)
        .Executes(() =>
        {
            var gitVersion = GitVersionTasks.GitVersion(s => s
                .SetOutput(GitVersionOutput.json)
                .SetProcessWorkingDirectory(RootDirectory)
            );
            Version = gitVersion.Result.MajorMinorPatch;
        });

    Target Clean => _ => _
        .DependsOn(Setup)
        .Executes(() =>
        {
            DotNetTasks.DotNetClean(c => c
                .SetProject(Solution)
                .SetVerbosity(DotNetVerbosity.minimal)
            );
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(r => r
                .SetProjectFile(Solution)
                .SetVerbosity(DotNetVerbosity.minimal)
            );
        });

    Target Compile => _ => _
        .DependsOn(Restore, ObtainVersion)
        .Consumes(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(s => s
                .SetNoRestore(true)
                .SetNoLogo(true)
                .SetProjectFile(Solution)
                .SetVersion(Version) // Use the calculated version
                .SetFileVersion(Version)
                .SetAssemblyVersion(Version)
                .SetInformationalVersion(Version)
                .SetVerbosity(DotNetVerbosity.minimal)
            );
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Consumes(Compile)
        .Executes(() =>
        {
            if (TestProjects.Count != 0)
            {
                foreach (var project in TestProjects)
                {
                    DotNetTasks.DotNetTest(s => s
                   .SetNoRestore(true)
                   .SetNoLogo(true)
                   .SetProjectFile(project)
                   .SetConfiguration(Configuration.Release)
                   .EnableCollectCoverage()
                   .SetResultsDirectory(TestReportDirectory)
                   .AddLoggers("trx")
                   .When(IsRunningAzurePipelines, _ => _
                       .SetProperty("ReportGenerator_BuildId", "$(Build.BuildId)")
                       .SetProperty("ReportGenerator_ReportType", "TextSummary;Html")
                       .SetProperty("ReportGenerator_SummaryFile", CoverageReport)
                   ));
                }
            }
            else
            {
                Log.Warning("skip testing stage in pipeline, found: {testingProjectsCount}", TestProjects.Count());
            }
        });

    Target PushCoverageTestReport => _ => _
        .DependsOn(Test)
        .Before(TearDown)
        .Consumes(Test)
        .Produces(CoverageReportDirectory / $"*.xml")
        .Executes(() =>
        {
            if (TestProjects.Count == 0)
            {
                Log.Warning("skip test report publishing.");
                return;
            }

            // Run Coverlet to generate code coverage report in Cobertura format
            foreach (var project in Solution.AllProjects)
            {
                var projectName = Path.GetFileNameWithoutExtension(project);
                var coverageOutput = CoverageReportDirectory / $"{projectName}.xml";

                CoverletTasks.Coverlet(s => s
                    .SetTarget(project)
                    .SetFormat(CoverletOutputFormat.cobertura)
                    .SetOutput(coverageOutput)
                );
            }

            // Generate HTML report from the Cobertura coverage report
            var reportDirectory = ArtifactsDirectory / "coverage-report";
            ReportGeneratorTasks.ReportGenerator(s => s
                .SetReports(CoverageReportDirectory)
                .SetReportTypes(ReportTypes.Html)
                .SetTargetDirectory(reportDirectory)
            );

            if (IsRunningAzurePipelines)
            {
                var testFiles = new[] { (TestReportDirectory + "/*.trx").ToString() }.ToList();
                AzurePipelines.PublishTestResults(title: "Test Results",
                    type: AzurePipelinesTestResultsType.VSTest,
                    files: testFiles,
                    mergeResults: true
                );
            }
        });
    Target TearDown => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            if (IsLocalBuild)
            {
                TestReportDirectory.DeleteDirectory();
                ArtifactsDirectory.DeleteDirectory();
                PublishDirectory.DeleteDirectory();
                CoverageReportDirectory.DeleteDirectory();
            }
        });
}