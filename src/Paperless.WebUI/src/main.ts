// Angular und unsere Seiten-Komponente laden
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';

// Die Angular-Anwendung starten und Startfehler anzeigen
bootstrapApplication(AppComponent).catch(function (error) {
    console.error(error);
});
