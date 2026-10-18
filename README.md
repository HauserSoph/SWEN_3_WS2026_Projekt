# Paperless

Unser Projekt für SWEN3 ist ein Dokumentenmanagement-System.

## Unser Stand für Sprint 1

Wir haben versucht, REST, Business-Logik, IoC und Mapping umzusetzen.
Bisher geht es um Dokument-Metadaten und Notizen, noch nicht um PDF-Dateiinhalte.
Die DAL und die Registrierung des Repositorys sind inzwischen im Code vorhanden.
Ob alles mit der echten Datenbank zusammenspielt, müssen wir noch gemeinsam prüfen.

## Backend-Tests und Validierung

Die API prüft Eingaben weiterhin mit `[Required]` und `[StringLength]`.
Die Business-Schicht behält die bestehende Textprüfung mit Trim und
`BusinessValidationException` bei. Zusätzlich werden fehlende Commands und
leere Dokument-IDs abgelehnt.

Dokumente und Notizen verwenden dieselben Pflichtfelder und Längengrenzen
auch im Domain-/Datenbankmodell: Titel 200, Dateiname 255, Notiztext 2000 Zeichen.
Der DbContext prüft neue und geänderte Objekte vor jedem Speichern mit
DataAnnotations. Dadurch werden ungültige Daten auch bei direkter DAL-Nutzung
abgelehnt. Datenbankänderungen an bestehenden Tabellen werden nicht automatisch
ausgeführt; die Modellgrenzen gelten beim Neuanlegen der Tabellen. Die Prüfung
vor dem Speichern greift auch bei einer bereits bestehenden Datenbank.

Aktuell bestehen 21 Tests. Sie prüfen ungültige Eingaben, Grenzwerte,
fehlende Commands, leere IDs, unveränderte Daten nach ungültigen Updates sowie
API-Attribute und DAL-Validierung. Die Business-Tests verwenden ein
Mock-Repository. Die DAL-Tests prüfen ungültige Daten vor dem Datenbankzugriff;
sie sind kein Nachweis für funktionierende PostgreSQL-Persistenz.

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

## Logging

Einfache Console.WriteLine-Aufrufe protokollieren API-Aufrufe, geladene Dokumente
und Notizen, erfolgreiches Erstellen/Ändern/Löschen, Datenbankzugriffe und Fehler.
Beim API-Start und beim Beenden werden ebenfalls Meldungen ausgegeben.
Fehlgeschlagene Datenbankzugriffe werden protokolliert und die Exception danach
weitergegeben. Validierungsfehler und fehlende Dokumente werden im bestehenden
BusinessExceptionHandler protokolliert; unerwartete API-Fehler ebenfalls.
IDs und Anzahlen werden ausgegeben, Titel und Notiztexte nicht.
Die Logs erscheinen in der Konsole oder über `docker compose logs -f api`.
Es gibt keinen eigenen Logger und keine zusätzliche Logging-Bibliothek.
