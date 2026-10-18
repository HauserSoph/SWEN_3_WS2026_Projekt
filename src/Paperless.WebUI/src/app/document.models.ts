export interface Document {
    id: string;
    title: string;
    fileName: string;
    createdAt: string;
}

export interface CreateDocumentRequest {
    title: string;
    fileName: string;
}

export type UpdateDocumentRequest = CreateDocumentRequest;

export interface DocumentNote {
    id: string;
    documentId: string;
    text: string;
    createdAt: string;
}

export interface CreateNoteRequest {
    text: string;
}

export interface ProblemDetails {
    type?: string;
    title?: string;
    status?: number;
    detail?: string;
    instance?: string;
}
