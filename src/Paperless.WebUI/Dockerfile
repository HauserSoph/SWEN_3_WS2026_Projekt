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
ENV NGINX_ENVSUBST_VARS='$API_BASE_URL'

COPY --from=build /app/dist/paperless-webui/browser/ /usr/share/nginx/html/

# Unsere Webserver-Konfiguration verwenden
ENV API_BASE_URL=http://api:8080
COPY nginx.conf /etc/nginx/templates/default.conf.template

# Die Webseite über HTTP bereitstellen
EXPOSE 80

# Nginx im Vordergrund starten, damit der Container weiterläuft
CMD ["nginx", "-g", "daemon off;"]
