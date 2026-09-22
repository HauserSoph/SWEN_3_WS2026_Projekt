namespace Paperless.UnitTests;

/// Kleiner Smoke-Test, der prueft, ob die Domain-Assembly eingebunden ist.
/// Business-, Mapping- und HTTP-Tests stehen inzwischen in eigenen Testklassen.
/// Plan nach DAL-Integration: echte Datenbankpersistenz separat pruefen; dieser Test
/// bleibt bewusst ein einfacher Assembly-Test und ersetzt keinen Funktionstest.

public class DomainAssemblyTests
{
    /// Checks that the solution references the expected domain assembly.
    [Fact]
    public void DomainAssembly_IsAvailable()
    {
        Assert.Equal("Paperless.Domain", typeof(Paperless.Domain.DomainAssembly).Assembly.GetName().Name);
    }
}
