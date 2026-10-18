import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DocumentApiService } from './document-api.service';
import { apiErrorMessage } from './api-error-message';
import { Document, DocumentNote } from './document.models';

@Component({
    selector: 'app-document-detail',
    imports: [RouterLink, FormsModule],
    template: `
        <h2>Dokumentdetails</h2>
        @if (loading) {
            <p>Dokument wird geladen …</p>
        } @else if (document) {
            <dl>
                <dt>ID</dt>
                <dd>{{ document.id }}</dd>
                <dt>Titel</dt>
                <dd>{{ document.title }}</dd>
                <dt>Dateiname</dt>
                <dd>{{ document.fileName }}</dd>
                <dt>Erstellt</dt>
                <dd>{{ document.createdAt }}</dd>
            </dl>

            <h3>Metadaten bearbeiten</h3>
            <form (ngSubmit)="updateDocument()" novalidate>
                <p>
                    <label for="title">Titel (maximal 200 Zeichen)</label><br>
                    <input id="title" name="title" type="text" [(ngModel)]="title" required maxlength="200">
                </p>
                <p>
                    <label for="fileName">Dateiname (maximal 255 Zeichen)</label><br>
                    <input id="fileName" name="fileName" type="text" [(ngModel)]="fileName" required maxlength="255">
                </p>
                <button type="submit" [disabled]="saving">Änderungen speichern</button>
                <button type="button" (click)="deleteDocument()" [disabled]="saving">Dokument löschen</button>
            </form>

            <h3>Notizen</h3>
            @if (notes.length === 0) {
                <p>Es sind noch keine Notizen vorhanden.</p>
            } @else {
                <ul>
                    @for (note of notes; track note.id) {
                        <li>{{ note.text }} ({{ note.createdAt }})</li>
                    }
                </ul>
            }
            <form (ngSubmit)="addNote()" novalidate>
                <p>
                    <label for="noteText">Neue Notiz (maximal 2000 Zeichen)</label><br>
                    <textarea id="noteText" name="noteText" [(ngModel)]="noteText" required maxlength="2000"></textarea>
                </p>
                <button type="submit" [disabled]="savingNote">Notiz hinzufügen</button>
            </form>
        } @else {
            <p role="alert">{{ errorMessage || 'Dokument nicht gefunden.' }}</p>
        }

        @if (message) {
            <p role="status">{{ message }}</p>
        }
        @if (errorMessage && document) {
            <p role="alert">{{ errorMessage }}</p>
        }
        <a routerLink="/">Zurück zum Dashboard</a>
    `
})
export class DocumentDetailComponent implements OnInit {
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly api = inject(DocumentApiService);
    document: Document | null = null;
    notes: DocumentNote[] = [];
    title = '';
    fileName = '';
    noteText = '';
    loading = true;
    saving = false;
    savingNote = false;
    message = '';
    errorMessage = '';

    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id');
        if (!id) {
            this.loading = false;
            this.errorMessage = 'Ungültige Dokument-ID.';
            return;
        }

        this.api.getDocument(id).subscribe({
            next: document => {
                this.document = document;
                this.title = document.title;
                this.fileName = document.fileName;
                this.loading = false;
                this.loadNotes(id);
            },
            error: error => {
                this.errorMessage = apiErrorMessage(error, 'Dokument konnte nicht geladen werden.');
                this.loading = false;
            }
        });
    }

    private loadNotes(documentId: string): void {
        this.api.getNotes(documentId).subscribe({
            next: notes => this.notes = notes,
            error: error => this.errorMessage = apiErrorMessage(error, 'Notizen konnten nicht geladen werden.')
        });
    }

    updateDocument(): void {
        if (!this.document || !this.title.trim() || !this.fileName.trim()) {
            this.errorMessage = 'Titel und Dateiname sind erforderlich.';
            return;
        }
        if (this.title.length > 200 || this.fileName.length > 255) {
            this.errorMessage = 'Titel oder Dateiname überschreitet die erlaubte Länge.';
            return;
        }

        this.saving = true;
        this.message = '';
        this.errorMessage = '';
        this.api.updateDocument(this.document.id, {
            title: this.title.trim(),
            fileName: this.fileName.trim()
        }).subscribe({
            next: document => {
                this.document = document;
                this.title = document.title;
                this.fileName = document.fileName;
                this.message = 'Änderungen wurden gespeichert.';
                this.saving = false;
            },
            error: error => {
                this.errorMessage = apiErrorMessage(error, 'Änderungen konnten nicht gespeichert werden.');
                this.saving = false;
            }
        });
    }

    deleteDocument(): void {
        if (!this.document) {
            return;
        }
        this.saving = true;
        this.errorMessage = '';
        this.api.deleteDocument(this.document.id).subscribe({
            next: () => this.router.navigateByUrl('/'),
            error: error => {
                this.errorMessage = apiErrorMessage(error, 'Dokument konnte nicht gelöscht werden.');
                this.saving = false;
            }
        });
    }

    addNote(): void {
        if (!this.document || !this.noteText.trim()) {
            this.errorMessage = 'Der Notiztext ist erforderlich.';
            return;
        }
        if (this.noteText.length > 2000) {
            this.errorMessage = 'Der Notiztext darf höchstens 2000 Zeichen haben.';
            return;
        }

        this.savingNote = true;
        this.errorMessage = '';
        this.api.addNote(this.document.id, { text: this.noteText.trim() }).subscribe({
            next: note => {
                this.notes = [...this.notes, note];
                this.noteText = '';
                this.message = 'Notiz wurde hinzugefügt.';
                this.savingNote = false;
            },
            error: error => {
                this.errorMessage = apiErrorMessage(error, 'Notiz konnte nicht hinzugefügt werden.');
                this.savingNote = false;
            }
        });
    }
}
