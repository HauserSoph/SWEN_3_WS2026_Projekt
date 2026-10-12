import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
// Ein Beispieldokument für Dashboard und Detailseite verwenden
export const sampleDocument = {
    id: '1',
    title: 'Beispielrechnung',
    fileName: 'rechnung.pdf'
};

// Die Dokumentübersicht und das vorhandene Formular anzeigen
@Component({
    selector: 'app-dashboard',
    imports: [RouterLink, FormsModule],
    template: `
        <h2>Dashboard</h2>
        <p>Beispieldaten - noch keine Verbindung zur Datenbank.</p>
        <p>
            <a [routerLink]="['/documents', document.id]">{{ document.title }}</a>
            - {{ document.fileName }}
        </p>

        <h2>Dokument-Metadaten eingeben</h2>

        <!-- Beim Absenden die Eingaben in der Komponente prüfen -->
        <form (ngSubmit)="checkInput()" novalidate>
            <p>
                <label for="title">Titel (maximal 200 Zeichen)</label><br>
                <input id="title" name="title" type="text" [(ngModel)]="title" required maxlength="200">
            </p>

            <p>
                <label for="fileName">Dateiname (maximal 255 Zeichen)</label><br>
                <input id="fileName" name="fileName" type="text" [(ngModel)]="fileName" required maxlength="255">
            </p>

            <button type="submit">Eingaben prüfen</button>
        </form>

        <p role="status">{{ message }}</p>
    `
})
export class DashboardComponent {
    document = sampleDocument;

    // Die Eingaben und die Meldung für das Formular speichern
    title = '';
    fileName = '';
    message = '';

    // Die Eingaben prüfen, bevor später ein Dokument gespeichert werden kann
    checkInput(): void {
        console.log('Formularprüfung gestartet.');
        this.message = '';

        if (this.title.trim().length === 0) {
            this.message = 'Bitte einen Titel eingeben.';
            console.warn(this.message);
            return;
        }

        if (this.title.length > 200) {
            this.message = 'Der Titel darf höchstens 200 Zeichen haben.';
            console.warn(this.message);
            return;
        }

        if (this.fileName.trim().length === 0) {
            this.message = 'Bitte einen Dateinamen eingeben.';
            console.warn(this.message);
            return;
        }

        if (this.fileName.length > 255) {
            this.message = 'Der Dateiname darf höchstens 255 Zeichen haben.';
            console.warn(this.message);
            return;
        }

        this.message = 'Die Eingaben sind gültig. Es wurde noch nichts gespeichert.';
        console.log('Formularprüfung erfolgreich.');
    }
}

