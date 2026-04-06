using NUnit.Framework;

namespace WindowsServicify.Domain.Tests;

[TestFixture]
public class WindowsServiceInstallHelperFactoryTests
{
    [Test]
    public void Create_WithLegacyTrue_ReturnsLegacyWindowsServiceInstallHelper()
    {
        var result = WindowsServiceInstallHelperFactory.Create(legacy: true);

        Assert.That(result, Is.InstanceOf<LegacyWindowsServiceInstallHelper>());
    }

    [Test]
    public void Create_WithLegacyFalse_ReturnsPowerShellWindowsServiceInstallHelper()
    {
        var result = WindowsServiceInstallHelperFactory.Create(legacy: false);

        Assert.That(result, Is.InstanceOf<PowerShellWindowsServiceInstallHelper>());
    }

    [Test]
    public void Create_WithLegacyTrue_ReturnsIWindowsServiceInstallHelper()
    {
        var result = WindowsServiceInstallHelperFactory.Create(legacy: true);

        Assert.That(result, Is.InstanceOf<IWindowsServiceInstallHelper>());
    }

    [Test]
    public void Create_WithLegacyFalse_ReturnsIWindowsServiceInstallHelper()
    {
        var result = WindowsServiceInstallHelperFactory.Create(legacy: false);

        Assert.That(result, Is.InstanceOf<IWindowsServiceInstallHelper>());
    }

    [Test]
    public void Create_WithLegacyTrue_ReturnsNewInstanceEachCall()
    {
        var first = WindowsServiceInstallHelperFactory.Create(legacy: true);
        var second = WindowsServiceInstallHelperFactory.Create(legacy: true);

        Assert.That(first, Is.Not.SameAs(second));
    }

    [Test]
    public void Create_WithLegacyFalse_ReturnsNewInstanceEachCall()
    {
        var first = WindowsServiceInstallHelperFactory.Create(legacy: false);
        var second = WindowsServiceInstallHelperFactory.Create(legacy: false);

        Assert.That(first, Is.Not.SameAs(second));
    }
}