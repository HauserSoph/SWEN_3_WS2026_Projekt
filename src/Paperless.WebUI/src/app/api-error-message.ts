import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from './document.models';

export function apiErrorMessage(error: unknown, fallback: string): string {
    if (!(error instanceof HttpErrorResponse)) {
        return fallback;
    }

    if (error.status === 0) {
        return 'Die API ist nicht erreichbar. Bitte Verbindung und Containerstatus prüfen.';
    }

    const problem = error.error as ProblemDetails | null;
    if (error.status === 400) {
        return problem?.detail ?? problem?.title ?? 'Die eingegebenen Daten sind ungültig.';
    }
    if (error.status === 404) {
        return problem?.detail ?? problem?.title ?? 'Das angeforderte Dokument wurde nicht gefunden.';
    }

    return problem?.detail ?? fallback;
}
