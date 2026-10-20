import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
    CreateDocumentRequest,
    CreateNoteRequest,
    Document,
    DocumentNote,
    UpdateDocumentRequest
} from './document.models';

@Injectable({ providedIn: 'root' })
export class DocumentApiService {
    private readonly http = inject(HttpClient);
    private readonly documentsUrl = '/api/documents';

    getDocuments(): Observable<Document[]> {
        return this.http.get<Document[]>(this.documentsUrl);
    }

    getDocument(id: string): Observable<Document> {
        return this.http.get<Document>(`${this.documentsUrl}/${id}`);
    }

    createDocument(request: CreateDocumentRequest): Observable<Document> {
        return this.http.post<Document>(this.documentsUrl, request);
    }

    updateDocument(id: string, request: UpdateDocumentRequest): Observable<Document> {
        return this.http.put<Document>(`${this.documentsUrl}/${id}`, request);
    }

    deleteDocument(id: string): Observable<void> {
        return this.http.delete<void>(`${this.documentsUrl}/${id}`);
    }

    getNotes(documentId: string): Observable<DocumentNote[]> {
        return this.http.get<DocumentNote[]>(`${this.documentsUrl}/${documentId}/notes`);
    }

    addNote(documentId: string, request: CreateNoteRequest): Observable<DocumentNote> {
        return this.http.post<DocumentNote>(`${this.documentsUrl}/${documentId}/notes`, request);
    }
}
