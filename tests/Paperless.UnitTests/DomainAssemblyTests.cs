namespace Paperless.UnitTests;

/// <summary>
/// Nur ein Smoke-Test, ob die Domain-Assembly überhaupt geladen wird.
/// Richtige Tests kommen mit den Repositories und Use Cases.
/// </summary>
public class DomainAssemblyTests
{
    [Fact]
    public void DomainAssembly_IsAvailable()
    {
        Assert.Equal("Paperless.Domain", typeof(Paperless.Domain.DomainAssembly).Assembly.GetName().Name);
    }
}
