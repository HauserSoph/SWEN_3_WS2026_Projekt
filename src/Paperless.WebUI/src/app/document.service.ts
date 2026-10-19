import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface DocumentDto {
    id: string;
    title: string;
    fileName: string;
    createdAt: string;
}

// Alle Dokument-Anfragen an einer Stelle sammeln.
@Injectable({ providedIn: 'root' })
export class DocumentService {
    private http = inject(HttpClient);

    getAll() {
        return this.http.get<DocumentDto[]>('/api/documents');
    }

    getById(id: string) {
        return this.http.get<DocumentDto>('/api/documents/' + encodeURIComponent(id));
    }

    upload(title: string, file: File) {
        const form = new FormData();
        form.append('title', title);
        form.append('file', file);
        // Den Content-Type samt Dateigrenze setzt der Browser selbst.
        return this.http.post<DocumentDto>('/api/documents', form);
    }
}
