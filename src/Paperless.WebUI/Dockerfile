FROM nginx:alpine

# Bestehende Default-Webseite entfernen
RUN rm -rf /usr/share/nginx/html/*

# WebUI ins Nginx Webroot kopieren
COPY www/ /usr/share/nginx/html/

# HTTP und HTTPS
EXPOSE 80 443

CMD ["nginx", "-g", "daemon off;"]