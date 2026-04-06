using NUnit.Framework;

namespace WindowsServicify.Domain.Tests;

/// <summary>
/// Validates the structure and content of the GitHub Actions build.yml workflow.
/// These tests ensure that critical workflow configuration is not accidentally changed.
/// </summary>
[TestFixture]
public class WorkflowValidationTests
{
    private string _workflowContent = string.Empty;

    [OneTimeSetUp]
    public void LoadWorkflow()
    {
        // Navigate from test output directory to repository root
        var dir = TestContext.CurrentContext.TestDirectory;
        while (dir != null && !Directory.Exists(Path.Combine(dir, ".github")))
        {
            dir = Directory.GetParent(dir)?.FullName;
        }

        Assert.That(dir, Is.Not.Null, "Could not find repository root with .github directory");

        var workflowPath = Path.Combine(dir!, ".github", "workflows", "build.yml");
        Assert.That(File.Exists(workflowPath), Is.True, $"Workflow file not found at {workflowPath}");

        _workflowContent = File.ReadAllText(workflowPath);
    }

    [Test]
    public void Workflow_HasPushTriggerOnMain()
    {
        Assert.That(_workflowContent, Does.Contain("push:"));
        Assert.That(_workflowContent, Does.Contain("branches: [main]"));
    }

    [Test]
    public void Workflow_HasWorkflowDispatchTrigger()
    {
        Assert.That(_workflowContent, Does.Contain("workflow_dispatch:"));
    }

    [Test]
    public void Workflow_HasBumpTypeInput_WithMajorMinorPatch()
    {
        Assert.That(_workflowContent, Does.Contain("bump:"));
        Assert.That(_workflowContent, Does.Contain("- patch"));
        Assert.That(_workflowContent, Does.Contain("- minor"));
        Assert.That(_workflowContent, Does.Contain("- major"));
    }

    [Test]
    public void Workflow_HasContentsWritePermission()
    {
        Assert.That(_workflowContent, Does.Contain("contents: write"));
    }

    [Test]
    public void Workflow_HasPrepareJob()
    {
        Assert.That(_workflowContent, Does.Contain("prepare:"));
    }

    [Test]
    public void Workflow_HasBuildAndReleaseJob()
    {
        Assert.That(_workflowContent, Does.Contain("build-and-release:"));
    }

    [Test]
    public void Workflow_PrepareJobOnlyRunsOnWorkflowDispatch()
    {
        // The prepare job must have a condition that limits it to workflow_dispatch
        var prepareIndex = _workflowContent.IndexOf("prepare:", StringComparison.Ordinal);
        Assert.That(prepareIndex, Is.GreaterThan(-1));

        var sectionAfterPrepare = _workflowContent.Substring(prepareIndex, 200);
        Assert.That(sectionAfterPrepare, Does.Contain("workflow_dispatch"));
    }

    [Test]
    public void Workflow_UsesFetchDepthZero()
    {
        Assert.That(_workflowContent, Does.Contain("fetch-depth: 0"));
    }

    [Test]
    public void Workflow_UsesFetchTagsTrue()
    {
        Assert.That(_workflowContent, Does.Contain("fetch-tags: true"));
    }

    [Test]
    public void Workflow_HasNuGetCaching()
    {
        Assert.That(_workflowContent, Does.Contain("actions/cache@v4"));
        Assert.That(_workflowContent, Does.Contain("~/.nuget/packages"));
        Assert.That(_workflowContent, Does.Contain("hashFiles('**/*.csproj')"));
    }

    [Test]
    public void Workflow_HasCoverageCollection()
    {
        Assert.That(_workflowContent, Does.Contain("XPlat Code Coverage"));
        Assert.That(_workflowContent, Does.Contain("coverlet.runsettings"));
    }

    [Test]
    public void Workflow_HasCoverageArtifactUpload()
    {
        Assert.That(_workflowContent, Does.Contain("upload-artifact@v4"));
        Assert.That(_workflowContent, Does.Contain("coverage-report"));
        Assert.That(_workflowContent, Does.Contain("coverage.cobertura.xml"));
    }

    [Test]
    public void Workflow_UsesWinX64Runtime()
    {
        Assert.That(_workflowContent, Does.Contain("win-x64"));
    }

    [Test]
    public void Workflow_UsesWindowsLatest()
    {
        Assert.That(_workflowContent, Does.Contain("windows-latest"));
    }

    [Test]
    public void Workflow_UsesSelfContainedPublish()
    {
        Assert.That(_workflowContent, Does.Contain("--self-contained true"));
        Assert.That(_workflowContent, Does.Contain("PublishSingleFile=true"));
    }

    [Test]
    public void Workflow_UsesVimtorActionZip()
    {
        Assert.That(_workflowContent, Does.Contain("vimtor/action-zip@v1"));
    }

    [Test]
    public void Workflow_SetsBuildTimeVersion()
    {
        Assert.That(_workflowContent, Does.Contain("-p:Version="));
    }

    [Test]
    public void Workflow_UsesSoftpropsGhRelease()
    {
        Assert.That(_workflowContent, Does.Contain("softprops/action-gh-release@v2"));
    }

    [Test]
    public void Workflow_HasSemverCalculationScript()
    {
        Assert.That(_workflowContent, Does.Contain("calculate-next-version.sh"));
    }

    [Test]
    public void Workflow_ReleaseStepsAreConditionalOnIsRelease()
    {
        // Publish, Delete PDB, ZIP, Tag, and Release steps must be conditional
        Assert.That(_workflowContent, Does.Contain("is_release == 'true'"));
    }

    [Test]
    public void Workflow_DoesNotTriggerReleaseOnPush()
    {
        // The build-and-release job should not create a release on push events
        // This is ensured by the is_release condition which only becomes true on workflow_dispatch
        var determineVersionIndex = _workflowContent.IndexOf("Determine version", StringComparison.Ordinal);
        Assert.That(determineVersionIndex, Is.GreaterThan(-1));

        var remainingLength = Math.Min(500, _workflowContent.Length - determineVersionIndex);
        var sectionAfterDetermineVersion = _workflowContent.Substring(determineVersionIndex, remainingLength);
        Assert.That(sectionAfterDetermineVersion, Does.Contain("workflow_dispatch"));
        Assert.That(sectionAfterDetermineVersion, Does.Contain("is_release=true"));
        Assert.That(sectionAfterDetermineVersion, Does.Contain("is_release=false"));
    }

    [Test]
    public void Workflow_DeletesPdbFiles()
    {
        Assert.That(_workflowContent, Does.Contain("Delete PDB files"));
        Assert.That(_workflowContent, Does.Contain("*.pdb"));
    }

    [Test]
    public void Workflow_CreatesGitTag()
    {
        Assert.That(_workflowContent, Does.Contain("Create Git tag"));
        Assert.That(_workflowContent, Does.Contain("git tag"));
        Assert.That(_workflowContent, Does.Contain("git push origin"));
    }

    [Test]
    public void Workflow_GeneratesReleaseNotes()
    {
        Assert.That(_workflowContent, Does.Contain("generate_release_notes: true"));
    }

    [Test]
    public void Workflow_UsesDotNet10()
    {
        Assert.That(_workflowContent, Does.Contain("10.0.x"));
    }
}