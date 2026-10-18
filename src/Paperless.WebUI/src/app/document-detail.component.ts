import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { sampleDocument } from './dashboard.component';

@Component({
    selector: 'app-document-detail',
    imports: [RouterLink],
    template: `
        <h2>Dokumentdetails</h2>
        <p>Beispieldaten - noch keine Verbindung zur Datenbank.</p>

        <!-- Die gefundenen Metadaten oder eine Fehlermeldung anzeigen -->
        @if (found) {
            <dl>
                <dt>ID</dt>
                <dd>{{ document.id }}</dd>
                <dt>Titel</dt>
                <dd>{{ document.title }}</dd>
                <dt>Dateiname</dt>
                <dd>{{ document.fileName }}</dd>
            </dl>
        } @else {
            <p>Dokument nicht gefunden.</p>
        }

        <a routerLink="/">Zurück zum Dashboard</a>
    `
})
export class DocumentDetailComponent implements OnInit {
    private route = inject(ActivatedRoute);
    document = sampleDocument;
    found = false;

    // Beim Öffnen der Seite die ID aus der URL lesen und das Dokument suchen
    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id');

        if (this.document.id === id) {
            this.found = true;
        }
    }
}
