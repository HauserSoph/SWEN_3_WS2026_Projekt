// Angular und die Unterstützung für Formulare laden
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

// Das HTML-Formular mit dieser Komponente verbinden
@Component({
    selector: 'app-root',
    imports: [FormsModule],
    templateUrl: './app.component.html'
})
export class AppComponent {
    // Die Eingaben und die Meldung für das Formular speichern
    title = '';
    fileName = '';
    message = '';

    // Die Eingaben prüfen, bevor später ein Dokument gespeichert werden kann
    checkInput(): void {
        // Alte Meldungen entfernen
        this.message = '';

        // Einen leeren Titel oder einen Titel nur aus Leerzeichen ablehnen
        if (this.title.trim().length === 0) {
            this.message = 'Bitte einen Titel eingeben.';
            return;
        }

        // Die maximale Titellänge aus dem Backend prüfen
        if (this.title.length > 200) {
            this.message = 'Der Titel darf höchstens 200 Zeichen haben.';
            return;
        }

        // Einen leeren Dateinamen oder einen Dateinamen nur aus Leerzeichen ablehnen
        if (this.fileName.trim().length === 0) {
            this.message = 'Bitte einen Dateinamen eingeben.';
            return;
        }

        // Die maximale Länge des Dateinamens aus dem Backend prüfen
        if (this.fileName.length > 255) {
            this.message = 'Der Dateiname darf höchstens 255 Zeichen haben.';
            return;
        }

        // Bestätigen, dass die Eingaben gültig sind
        this.message = 'Die Eingaben sind gültig. Es wurde noch nichts gespeichert.';
    }
}
