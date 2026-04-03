using NUnit.Framework;

namespace WindowsServicify.Domain.Tests;

[TestFixture]
public class ResultTests
{
    [Test]
    public void Success_ReturnsIsSuccessTrue()
    {
        var result = Result<string>.Success("value");

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void Success_ReturnsCorrectValue()
    {
        var result = Result<string>.Success("test-value");

        Assert.That(result.Value, Is.EqualTo("test-value"));
    }

    [Test]
    public void Success_HasEmptyErrorMessage()
    {
        var result = Result<string>.Success("value");

        Assert.That(result.ErrorMessage, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Failure_ReturnsIsSuccessFalse()
    {
        var result = Result<string>.Failure("error");

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public void Failure_ReturnsCorrectErrorMessage()
    {
        var result = Result<string>.Failure("Something went wrong");

        Assert.That(result.ErrorMessage, Is.EqualTo("Something went wrong"));
    }

    [Test]
    public void Failure_ValueIsDefault()
    {
        var result = Result<int>.Failure("error");

        Assert.That(result.Value, Is.EqualTo(default(int)));
    }

    [Test]
    public void Success_WithIntegerType_ReturnsCorrectValue()
    {
        var result = Result<int>.Success(42);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public void Success_WithRecordType_ReturnsCorrectValue()
    {
        var parameters = new ConsoleCommandLineParameters(true, false, false, false, false, false);
        var result = Result<ConsoleCommandLineParameters>.Success(parameters);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Configure, Is.True);
    }
}
