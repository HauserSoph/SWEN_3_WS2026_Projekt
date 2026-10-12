# Nginx als Webserver verwenden
FROM nginx:alpine

# Bestehende Default-Webseite entfernen
RUN rm -rf /usr/share/nginx/html/*

# WebUI ins Nginx Webroot kopieren
COPY www/ /usr/share/nginx/html/

# Unsere Webserver-Konfiguration verwenden
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Die Webseite über HTTP bereitstellen
EXPOSE 80

# Nginx im Vordergrund starten, damit der Container weiterläuft
CMD ["nginx", "-g", "daemon off;"]
