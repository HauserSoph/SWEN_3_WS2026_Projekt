# Paperless

Dokumentenmanagement-System (SWEN3, WS 2026): Archivierung, OCR, Volltextsuche und KI-Zusammenfassungen.

**Stack:** C# / .NET 10 / ASP.NET Core

## Struktur

```
src/Paperless.Api        REST-Server
src/Paperless.Business   Fachlogik
src/Paperless.Dal        Datenzugriff
src/Paperless.Domain     Entitäten
tests/Paperless.UnitTests
```

## Loslegen

Voraussetzung: [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
dotnet build Paperless.sln
dotnet test Paperless.sln
dotnet run --project src/Paperless.Api
```

Health-Check: `GET http://localhost:5257/health` → `ok`
