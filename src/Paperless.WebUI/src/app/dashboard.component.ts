import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DocumentDto, DocumentService } from './document.service';

// Die Dokumentübersicht und ein einfaches Upload-Formular anzeigen.
@Component({
    selector: 'app-dashboard',
    imports: [RouterLink, FormsModule],
    template: `
        <h2>Dashboard</h2>
        <button type="button" (click)="loadDocuments()" [disabled]="loading() || uploading()">Aktualisieren</button>
        @if (loading()) {
            <p>Dokumente werden geladen.</p>
        }
        <p role="alert">{{ loadError() }}</p>
        @for (document of documents(); track document.id) {
            <p>
                <a [routerLink]="['/documents', document.id]">{{ document.title }}</a>
                - {{ document.fileName }}
            </p>
        } @empty {
            @if (!loading() && !loadError()) {
                <p>Noch keine Dokumente vorhanden.</p>
            }
        }

        <h2>Dokument hochladen</h2>
        <form (ngSubmit)="uploadDocument(fileInput)" novalidate>
            <p>
                <label for="title">Titel (maximal 200 Zeichen)</label><br>
                <input id="title" name="title" type="text" [(ngModel)]="title" required maxlength="200" [disabled]="uploading()">
            </p>
            <p>
                <label for="file">Datei (maximal 49 MiB)</label><br>
                <input #fileInput id="file" name="file" type="file" (change)="selectFile($event)" required [disabled]="uploading()">
            </p>
            <button type="submit" [disabled]="uploading()">{{ uploading() ? 'Wird hochgeladen …' : 'Hochladen' }}</button>
        </form>
        <p role="status">{{ message() }}</p>
    `
})
export class DashboardComponent implements OnInit {
    private service = inject(DocumentService);
    documents = signal<DocumentDto[]>([]);
    loading = signal(false);
    uploading = signal(false);
    loadError = signal('');
    message = signal('');
    title = '';
    file: File | null = null;

    ngOnInit(): void {
        this.loadDocuments();
    }

    loadDocuments(): void {
        this.loading.set(true);
        this.loadError.set('');
        this.service.getAll().subscribe({
            next: documents => {
                this.documents.set(documents);
                this.loading.set(false);
            },
            error: () => {
                this.loadError.set('Die Dokumente konnten nicht geladen werden. Bitte erneut versuchen.');
                this.loading.set(false);
            }
        });
    }

    selectFile(event: Event): void {
        const input = event.target as HTMLInputElement;
        this.file = input.files?.item(0) ?? null;
        this.message.set('');
    }

    // Erst die Eingaben prüfen, dann die Datei an die API senden.
    uploadDocument(fileInput: HTMLInputElement): void {
        if (this.uploading()) {
            return;
        }
        this.message.set('');
        if (this.title.trim().length === 0 || this.title.trim().length > 200) {
            this.message.set('Bitte einen Titel mit 1 bis 200 Zeichen eingeben.');
            return;
        }
        if (this.file === null || this.file.size === 0) {
            this.message.set('Bitte eine nicht leere Datei auswählen.');
            return;
        }
        if (this.file.name.length > 255) {
            this.message.set('Der Dateiname darf höchstens 255 Zeichen haben.');
            return;
        }
        // Etwas Platz für die Formulardaten unter dem API-Limit von 50 MiB lassen.
        if (this.file.size > 49 * 1024 * 1024) {
            this.message.set('Die Datei darf höchstens 49 MiB groß sein.');
            return;
        }
        this.uploading.set(true);
        this.service.upload(this.title.trim(), this.file).subscribe({
            next: () => {
                this.message.set('Upload angenommen. Die Metadaten wurden gespeichert und der Verarbeitungsauftrag wurde an RabbitMQ gesendet.');
                this.title = '';
                this.file = null;
                fileInput.value = '';
                this.uploading.set(false);
                this.loadDocuments();
            },
            error: () => {
                this.message.set('Der Upload konnte nicht bestätigt werden. Bitte die Dokumentliste aktualisieren und den Server prüfen.');
                this.uploading.set(false);
            }
        });
    }
}
