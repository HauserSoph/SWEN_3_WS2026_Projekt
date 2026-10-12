import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

// Den Platz für die aktuelle Seite bereitstellen
@Component({
    selector: 'app-root',
    imports: [RouterOutlet],
    template: `
        <h1>Paperless</h1>
        <!-- Angular zeigt hier das Dashboard oder die Detailseite an -->
        <main>
            <router-outlet></router-outlet>
        </main>
    `
})
export class AppComponent {
}
