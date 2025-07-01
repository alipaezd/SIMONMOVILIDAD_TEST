# frontend/Dockerfile

FROM node:18-alpine AS build
WORKDIR /app

COPY package*.json ./

RUN npm install
RUN npm install react-leaflet leaflet recharts
RUN npm install @microsoft/signalr
RUN npm install react-icons
RUN npm install idb
COPY . .
RUN npm run build
RUN npm install react-router-dom
RUN npm install axios
FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf 
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
