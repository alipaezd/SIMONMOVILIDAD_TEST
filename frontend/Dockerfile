# frontend/Dockerfile

FROM node:18-alpine AS build
WORKDIR /app

# Copia sólo package.json (y el lock si existe)
COPY package*.json ./

# Usa npm install en lugar de npm ci
RUN npm install
RUN npm install react-leaflet leaflet recharts
RUN npm install @microsoft/signalr
RUN npm install react-icons
# Copia el resto y construye
COPY . .
RUN npm run build
RUN npm install react-router-dom
RUN npm install axios
FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf 
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
