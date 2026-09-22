namespace Paperless.Domain;

/// Absichtlich leere Markerklasse, mit der der Smoke-Test die Domain-Assembly erkennt.
/// Document, DocumentNote und IDocumentRepository stehen bereits in eigenen Dateien.
/// Plan: weitere benoetigte Fachmodelle ebenfalls hier im Domain-Projekt ergaenzen,
/// ohne HTTP-, Datenbank- oder Messaging-Abhaengigkeiten.
public static class DomainAssembly
{
}
