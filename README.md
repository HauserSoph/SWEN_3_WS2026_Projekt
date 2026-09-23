# Paperless

Unser Projekt für SWEN3 ist ein Dokumentenmanagement-System.

## Unser Stand für Sprint 1

Wir haben versucht, REST, Business-Logik, IoC und Mapping umzusetzen.
Bisher geht es um Dokument-Metadaten und Notizen, noch nicht um PDF-Dateiinhalte.
Die DAL und die Registrierung des Repositorys sind inzwischen im Code vorhanden.
Ob alles mit der echten Datenbank zusammenspielt, müssen wir noch gemeinsam prüfen.

## Unsere zwei Tests

- Ein Dokument mit Titel wird zum Speichern an das Repository übergeben.
- Ein Dokument ohne Titel wird abgelehnt und nicht gespeichert.

Die Tests verwenden ein Mock-Repository statt einer Datenbank.
Sie prüfen nur diese zwei Fälle, nicht alle Endpunkte oder die echte Speicherung.

**Visual Studio:** Test → Test-Explorer → Alle Tests ausführen.

Oder im Ordner mit `Paperless.sln`:

```powershell
dotnet test Paperless.sln
```

## Anwendung starten

**Mit dem vorhandenen Docker-Setup:** Docker Desktop starten und im Solution-Ordner ausführen:

```powershell
docker compose up --build
```

Danach im Browser [http://localhost:8080/health](http://localhost:8080/health) öffnen.
Dort sollte `ok` stehen. Unter [http://localhost:8080/api/documents](http://localhost:8080/api/documents) werden die Dokumente abgefragt.
`/health` allein prüft noch keine Datenbankverbindung.

**Direkt in Visual Studio:** Rechtsklick auf `Paperless.Api` → Als Startprojekt festlegen.
Das Startprofil `http` auswählen und mit F5 starten.
Dann [http://localhost:5257/health](http://localhost:5257/health) öffnen.

Für Dokument-Anfragen braucht dieser direkte Start PostgreSQL unter `localhost:5432`
mit den Einstellungen aus `appsettings.json`.
Der aktuelle Compose-Datenbankdienst gibt diesen Port nicht an Windows frei.
Deshalb ist ein nur im Compose-Netz laufendes PostgreSQL für diesen lokalen Start nicht ausreichend.

Das Mapping verwendet AutoMapper 15 wie in den Folien. Ein vorhandener Lizenzschlüssel kann über die Umgebungsvariable `AUTOMAPPER_KEY` gesetzt werden.
