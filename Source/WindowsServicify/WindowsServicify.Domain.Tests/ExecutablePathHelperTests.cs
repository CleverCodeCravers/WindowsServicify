using NUnit.Framework;

namespace WindowsServicify.Domain.Tests;

[TestFixture]
public class ExecutablePathHelperTests
{
    [Test]
    public void GetExecutableFilePath_ReturnsNonNullPath()
    {
        var result = ExecutablePathHelper.GetExecutableFilePath();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void GetExecutableFilePath_ReturnsPathEndingWithExe()
    {
        var result = ExecutablePathHelper.GetExecutableFilePath();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.EndWith(".exe").IgnoreCase
            .Or.EndsWith(".dll").IgnoreCase);
    }

    [Test]
    public void GetExecutablePath_ReturnsNonEmptyDirectory()
    {
        var result = ExecutablePathHelper.GetExecutablePath();

        Assert.That(result, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void GetExecutablePath_ReturnsExistingDirectory()
    {
        var result = ExecutablePathHelper.GetExecutablePath();

        Assert.That(Directory.Exists(result), Is.True);
    }

    [Test]
    public void GetExecutablePath_ReturnsParentDirectoryOfExecutableFile()
    {
        var filePath = ExecutablePathHelper.GetExecutableFilePath();
        var dirPath = ExecutablePathHelper.GetExecutablePath();

        var expectedDir = Path.GetDirectoryName(filePath);
        Assert.That(dirPath, Is.EqualTo(expectedDir));
    }
}