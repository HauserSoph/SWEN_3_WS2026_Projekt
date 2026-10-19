// Angular und unsere Seiten-Komponente laden
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideRouter, Routes } from '@angular/router';
import { DashboardComponent } from './app/dashboard.component';
import { DocumentDetailComponent } from './app/document-detail.component';
import { provideHttpClient } from '@angular/common/http';

// URL-Pfade mit den passenden Seiten verbinden
const routes: Routes = [
    { path: '', component: DashboardComponent, title: 'Paperless - Dashboard' },
    { path: 'documents/:id', component: DocumentDetailComponent, title: 'Paperless - Dokumentdetails' },
    { path: '**', redirectTo: '' }
];

// Die Angular-Anwendung starten und Startfehler anzeigen
bootstrapApplication(AppComponent, {
    providers: [provideRouter(routes), provideHttpClient()]
}).catch(function (error) {
    console.error(error);
});
