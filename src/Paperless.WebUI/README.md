# Paperless WebUI - Schritt 2

Den Ordner `Paperless.WebUI` in VS Code öffnen. Im Terminal ausführen:

```powershell
# Die Angular-Abhängigkeiten installieren
npm ci

# Den Entwicklungsserver starten
npm start
```

Die Seite ist unter http://localhost:4200 erreichbar.
Node.js 24.15 oder neuer innerhalb der Version 24 wird benötigt.

Das Formular prüft Titel und Dateiname: Pflichtfelder, Leerzeichen und maximale
Längen von 200 beziehungsweise 255 Zeichen. Es speichert noch nichts.

Zum Prüfen leere Felder, nur Leerzeichen und gültige Werte ausprobieren.
Die HTML-Felder begrenzen die Eingabelänge. Die TypeScript-Methode prüft die
Längen zusätzlich. Bei gültigen Werten wird eine Bestätigung angezeigt.

`package.json` enthält Abhängigkeiten und Startbefehle. `angular.json` legt fest,
wie Angular gebaut und gestartet wird. `tsconfig.json` enthält die
TypeScript-Einstellungen. `package.json` erlaubt keine Kommentare;
`tsconfig.json` enthält kurze Kommentare zu den Einstellungen.


```powershell
# Prüfen, ob die Angular-Anwendung gebaut werden kann
npm run build
```

Der bisherige Docker-Container zeigt weiterhin die Testseite aus `www`.
Die Angular-Seite wird in diesem Schritt mit `npm start` gestartet.
