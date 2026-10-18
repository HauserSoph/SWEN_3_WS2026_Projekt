# Die Angular-Anwendung bauen
FROM node:24-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

# Die gebaute Webseite mit Nginx ausliefern
FROM nginx:alpine

# Bestehende Default-Webseite entfernen
RUN rm -rf /usr/share/nginx/html/*

COPY --from=build /app/dist/paperless-webui/browser/ /usr/share/nginx/html/

# Unsere Webserver-Konfiguration verwenden
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Die Webseite über HTTP bereitstellen
EXPOSE 80

# Nginx im Vordergrund starten, damit der Container weiterläuft
CMD ["nginx", "-g", "daemon off;"]
