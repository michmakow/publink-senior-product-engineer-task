FROM node:22-alpine AS build
WORKDIR /app

COPY src/frontend/package*.json ./
RUN npm ci

COPY src/frontend ./

ARG VITE_API_BASE_URL=http://localhost:5080
ARG VITE_AUDIT_API_KEY=local-dev-audit-key
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_AUDIT_API_KEY=$VITE_AUDIT_API_KEY

RUN npm run build

FROM nginx:1.27-alpine AS runtime
COPY infrastructure/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
