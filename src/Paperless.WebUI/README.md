# Paperless WebUI

## Mit Docker starten

Im Ordner mit `docker-compose.yml` ausführen:

```powershell
docker compose up --build -d
```

Danach http://localhost/ öffnen. Nginx leitet `/api/` an das Backend weiter.
Die Adresse steht in `API_BASE_URL` und ist standardmäßig `http://api:8080`.

## Lokal entwickeln

Node.js 24.15 oder neuer innerhalb der Version 24 wird benötigt.
Im Ordner `src/Paperless.WebUI` ausführen:

```powershell
npm ci
npm start
```

Die Entwicklungsseite ist unter http://localhost/ erreichbar.
`proxy.conf.json` leitet API-Anfragen an http://localhost/ weiter.
Das Backend mit PostgreSQL und RabbitMQ muss dafür laufen.

## Dokumente hochladen und anzeigen

Das Dashboard lädt die echten Dokument-Metadaten aus der API.
Zum Hochladen einen Titel und eine nicht leere Datei auswählen und auf
„Hochladen“ klicken. Der Titel darf höchstens 200 Zeichen und der Dateiname
höchstens 255 Zeichen haben. Das Frontend erlaubt Dateien bis 49 MiB, damit
unter dem API-Limit von 50 MiB noch Platz für die Formulardaten bleibt.

Nach Annahme des Uploads wird die Liste erneut geladen. Der Link eines Dokuments
öffnet seine Metadaten aus der Datenbank. „Aktualisieren“ lädt die Liste erneut.
Ladezustände und Fehler werden auf der Seite angezeigt.

Die API speichert die Metadaten und schickt den Verarbeitungsauftrag an RabbitMQ.
Die Datei wird im Hintergrund verschoben. Die Upload-Bestätigung bedeutet daher
noch nicht, dass diese Verarbeitung abgeschlossen ist. Ein Verarbeitungsstatus
und ein Dateidownload sind in dieser Konzeptdemo nicht enthalten.

`document.service.ts` sammelt die HTTP-Anfragen. Die Komponenten enthalten
weiterhin einfache HTML-Templates und die Eingabeprüfung. Angular-Signale sorgen
dafür, dass Ergebnisse der asynchronen Anfragen direkt angezeigt werden.

## Prüfen

```powershell
npm run build
```

Für einen manuellen Durchlauf eine kleine Datei hochladen, den neuen Eintrag
öffnen und die Seite neu laden. Der Eintrag muss weiterhin vorhanden sein.
Die verschobene Datei liegt bei erfolgreicher Hintergrundverarbeitung unter
`.data/uploads/processed` im Projektordner.
