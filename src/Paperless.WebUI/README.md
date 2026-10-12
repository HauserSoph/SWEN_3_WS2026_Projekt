# Paperless WebUI - Schritt 3

Den Ordner `Paperless.WebUI` in VS Code öffnen. Im Terminal ausführen:

```powershell
# Die Angular-Abhängigkeiten installieren
npm ci

# Den Entwicklungsserver starten
npm start
```

Die Seite ist unter http://localhost/ erreichbar.
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

## Dashboard und Detailseiten

Das Dashboard unter `/` zeigt ein Beispieldokument.
Der Link öffnet `/documents/1`. Unbekannte Dokument-IDs zeigen
eine Meldung. Die Daten sind nur Beispiele; die REST-Anbindung kommt noch.

`main.ts` ordnet jeder URL eine Komponente zu. `routerLink` wechselt die Seite,
und `router-outlet` zeigt sie an. Die Detailseite liest die ID mit `ActivatedRoute`.
Ein einfaches `if` prüft, ob die ID zum Beispieldokument passt.
Das Beispieldokument steht in `dashboard.component.ts` und wird von der Detailseite importiert.
Das HTML steht direkt im `template` der jeweiligen Komponente. Das Formular und
seine Eingabeprüfung befinden sich in `dashboard.component.ts`.

## Mit Nginx starten

Im Ordner mit `docker-compose.yml` ausführen:

```powershell
docker compose up --build -d webui
```

Danach http://localhost/ öffnen. Der Dockerfile baut Angular mit Node.js und
kopiert die fertigen Dateien in den Nginx-Container. Nginx liefert bei Angular-URLs
die `index.html`, damit auch ein direkter Aufruf oder Neuladen der Detailseite klappt.

