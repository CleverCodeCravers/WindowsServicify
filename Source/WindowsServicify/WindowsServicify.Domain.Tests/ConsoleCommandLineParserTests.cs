using NUnit.Framework;

namespace WindowsServicify.Domain.Tests;

[TestFixture]
public class ConsoleCommandLineParserTests
{
    private ConsoleCommandLineParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new ConsoleCommandLineParser();
    }

    // --- Single option tests ---

    [Test]
    public void Parse_WithConfigureFlag_SetsConfigureToTrue()
    {
        var result = _parser.Parse(new[] { "--configure" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Configure, Is.True);
        Assert.That(result.Value.Install, Is.False);
        Assert.That(result.Value.Uninstall, Is.False);
        Assert.That(result.Value.Testrun, Is.False);
        Assert.That(result.Value.Legacy, Is.False);
        Assert.That(result.Value.Help, Is.False);
    }

    [Test]
    public void Parse_WithInstallFlag_SetsInstallToTrue()
    {
        var result = _parser.Parse(new[] { "--install" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Install, Is.True);
        Assert.That(result.Value.Configure, Is.False);
        Assert.That(result.Value.Uninstall, Is.False);
        Assert.That(result.Value.Testrun, Is.False);
        Assert.That(result.Value.Legacy, Is.False);
        Assert.That(result.Value.Help, Is.False);
    }

    [Test]
    public void Parse_WithUninstallFlag_SetsUninstallToTrue()
    {
        var result = _parser.Parse(new[] { "--uninstall" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Uninstall, Is.True);
        Assert.That(result.Value.Configure, Is.False);
        Assert.That(result.Value.Install, Is.False);
        Assert.That(result.Value.Testrun, Is.False);
        Assert.That(result.Value.Legacy, Is.False);
        Assert.That(result.Value.Help, Is.False);
    }

    [Test]
    public void Parse_WithTestrunFlag_SetsTestrunToTrue()
    {
        var result = _parser.Parse(new[] { "--testrun" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Testrun, Is.True);
        Assert.That(result.Value.Configure, Is.False);
        Assert.That(result.Value.Install, Is.False);
        Assert.That(result.Value.Uninstall, Is.False);
        Assert.That(result.Value.Legacy, Is.False);
        Assert.That(result.Value.Help, Is.False);
    }

    [Test]
    public void Parse_WithLegacyFlag_SetsLegacyToTrue()
    {
        var result = _parser.Parse(new[] { "--legacy" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Legacy, Is.True);
        Assert.That(result.Value.Configure, Is.False);
        Assert.That(result.Value.Install, Is.False);
        Assert.That(result.Value.Uninstall, Is.False);
        Assert.That(result.Value.Testrun, Is.False);
        Assert.That(result.Value.Help, Is.False);
    }

    [Test]
    public void Parse_WithHelpFlag_SetsHelpToTrue()
    {
        var result = _parser.Parse(new[] { "--help" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Help, Is.True);
        Assert.That(result.Value.Configure, Is.False);
        Assert.That(result.Value.Install, Is.False);
        Assert.That(result.Value.Uninstall, Is.False);
        Assert.That(result.Value.Testrun, Is.False);
        Assert.That(result.Value.Legacy, Is.False);
    }

    // --- Combination tests ---

    [Test]
    public void Parse_WithInstallAndLegacy_SetsBothToTrue()
    {
        var result = _parser.Parse(new[] { "--install", "--legacy" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Install, Is.True);
        Assert.That(result.Value.Legacy, Is.True);
        Assert.That(result.Value.Configure, Is.False);
        Assert.That(result.Value.Uninstall, Is.False);
        Assert.That(result.Value.Testrun, Is.False);
        Assert.That(result.Value.Help, Is.False);
    }

    [Test]
    public void Parse_WithUninstallAndLegacy_SetsBothToTrue()
    {
        var result = _parser.Parse(new[] { "--uninstall", "--legacy" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Uninstall, Is.True);
        Assert.That(result.Value.Legacy, Is.True);
    }

    // --- Error cases ---

    [Test]
    public void Parse_WithEmptyArgs_ReturnsFailure()
    {
        var result = _parser.Parse(Array.Empty<string>());

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Empty);
    }

    [Test]
    public void Parse_WithUnknownArgument_ReturnsFailure()
    {
        var result = _parser.Parse(new[] { "--unknown" });

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("--unknown"));
    }

    [Test]
    public void Parse_WithMixedValidAndInvalidArgs_ReturnsFailure()
    {
        var result = _parser.Parse(new[] { "--install", "--foo" });

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("--foo"));
    }

    // --- Case insensitivity ---

    [Test]
    public void Parse_WithUpperCaseFlag_IsCaseInsensitive()
    {
        var result = _parser.Parse(new[] { "--INSTALL" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Install, Is.True);
    }

    [Test]
    public void Parse_WithMixedCaseFlag_IsCaseInsensitive()
    {
        var result = _parser.Parse(new[] { "--Help" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Help, Is.True);
    }

    // --- GetCommandsList tests ---

    [Test]
    public void GetCommandsList_ReturnsAllSixCommands()
    {
        var commands = _parser.GetCommandsList();

        Assert.That(commands, Has.Length.EqualTo(6));
    }

    [Test]
    public void GetCommandsList_ContainsConfigureCommand()
    {
        var commands = _parser.GetCommandsList();

        Assert.That(commands.Any(c => c.Name == "--configure"), Is.True);
    }

    [Test]
    public void GetCommandsList_ContainsInstallCommand()
    {
        var commands = _parser.GetCommandsList();

        Assert.That(commands.Any(c => c.Name == "--install"), Is.True);
    }

    [Test]
    public void GetCommandsList_ContainsUninstallCommand()
    {
        var commands = _parser.GetCommandsList();

        Assert.That(commands.Any(c => c.Name == "--uninstall"), Is.True);
    }

    [Test]
    public void GetCommandsList_ContainsTestrunCommand()
    {
        var commands = _parser.GetCommandsList();

        Assert.That(commands.Any(c => c.Name == "--testrun"), Is.True);
    }

    [Test]
    public void GetCommandsList_ContainsLegacyCommand()
    {
        var commands = _parser.GetCommandsList();

        Assert.That(commands.Any(c => c.Name == "--legacy"), Is.True);
    }

    [Test]
    public void GetCommandsList_ContainsHelpCommand()
    {
        var commands = _parser.GetCommandsList();

        Assert.That(commands.Any(c => c.Name == "--help"), Is.True);
    }

    [Test]
    public void GetCommandsList_AllCommandsHaveDescriptions()
    {
        var commands = _parser.GetCommandsList();

        Assert.That(commands.All(c => !string.IsNullOrWhiteSpace(c.Description)), Is.True);
    }

    // --- Edge cases ---

    [Test]
    public void Parse_WithAllFlags_SetsAllToTrue()
    {
        var result = _parser.Parse(new[]
        {
            "--configure", "--install", "--uninstall",
            "--testrun", "--legacy", "--help"
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Configure, Is.True);
        Assert.That(result.Value.Install, Is.True);
        Assert.That(result.Value.Uninstall, Is.True);
        Assert.That(result.Value.Testrun, Is.True);
        Assert.That(result.Value.Legacy, Is.True);
        Assert.That(result.Value.Help, Is.True);
    }

    [Test]
    public void Parse_WithDuplicateFlags_Succeeds()
    {
        var result = _parser.Parse(new[] { "--install", "--install" });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Install, Is.True);
    }

    [Test]
    public void Parse_WithArgumentWithoutDashes_ReturnsFailure()
    {
        var result = _parser.Parse(new[] { "install" });

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public void Parse_WithSingleDash_ReturnsFailure()
    {
        var result = _parser.Parse(new[] { "-install" });

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public void Parse_WithEmptyString_ReturnsFailure()
    {
        var result = _parser.Parse(new[] { "" });

        Assert.That(result.IsSuccess, Is.False);
    }
}
