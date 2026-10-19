import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DocumentApiService } from './document-api.service';
import { apiErrorMessage } from './api-error-message';
import { Document } from './document.models';
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
        @if (loading()) {
            <p>Dokumente werden geladen …</p>
        } @else if (documents().length === 0) {
            <p>Es sind noch keine Dokumente vorhanden.</p>
        } @else {
            <ul>
                @for (document of documents(); track document.id) {
                    <li>
                        <a [routerLink]="['/documents', document.id]">{{ document.title }}</a>
                        - {{ document.fileName }}
                    </li>
                }
            </ul>
        }

        @if (errorMessage()) {
            <p role="alert">{{ errorMessage() }}</p>
        }

        <h2>Dokument erstellen</h2>


        <!-- Beim Absenden die Eingaben in der Komponente prüfen -->
        <form (ngSubmit)="createDocument()" novalidate>
            <p>
                <label for="title">Titel (maximal 200 Zeichen)</label><br>
                <input id="title" name="title" type="text" [(ngModel)]="title" required maxlength="200">
            </p>

            <p>
                <label for="fileName">Dateiname (maximal 255 Zeichen)</label><br>
                <input id="fileName" name="fileName" type="text" [(ngModel)]="fileName" required maxlength="255">
            </p>

            <button type="submit" [disabled]="saving()">Dokument erstellen</button>
        </form>

        <p role="status">{{ message }}</p>
    `
})
export class DashboardComponent implements OnInit {
    private readonly api = inject(DocumentApiService);
    documents = signal<Document[]>([]);
    loading = signal(true);
    saving = signal(false);
    errorMessage = signal('');

    // Die Eingaben und die Meldung für das Formular speichern
    title = '';
    fileName = '';
    message = '';

    ngOnInit(): void {
        this.loadDocuments();
    }

    private loadDocuments(): void {
        this.loading.set(true);
        this.errorMessage.set('');
        this.api.getDocuments().subscribe({
            next: documents => {
                this.documents.set(Array.isArray(documents) ? documents : []);
                this.loading.set(false);
            },
            error: error => {
                this.errorMessage.set(apiErrorMessage(error, 'Dokumente konnten nicht geladen werden.'));
                this.loading.set(false);
            }
        });
    }

    createDocument(): void {
        this.message = '';
        this.errorMessage.set('');

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

        this.saving.set(true);
        this.api.createDocument({ title: this.title.trim(), fileName: this.fileName.trim() }).subscribe({
            next: document => {
                this.documents.update(documents => [document, ...documents]);
                this.title = '';
                this.fileName = '';
                this.message = 'Dokument wurde erstellt.';
                this.saving.set(false);
            },
            error: error => {
                this.errorMessage.set(apiErrorMessage(error, 'Dokument konnte nicht erstellt werden.'));
                this.saving.set(false);
            }
        });
    }
}

