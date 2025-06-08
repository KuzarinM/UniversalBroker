# Этап сборки приложения
FROM node:20-alpine AS build
WORKDIR /app

# Копируем файлы зависимостей и устанавливаем их
COPY package*.json ./
RUN npm ci

# Копируем исходный код и собираем проект
COPY . .
RUN npm run build

# Этап запуска приложения
FROM nginx:stable-alpine AS production
LABEL maintainer="your-email@example.com"

# Копируем собранные файлы из этапа сборки
COPY --from=build /app/dist /usr/share/nginx/html

# Удаляем дефолтную конфигурацию Nginx
RUN rm /etc/nginx/conf.d/default.conf

# Копируем кастомную конфигурацию (опционально)
COPY nginx.conf /etc/nginx/conf.d

# Открываем порт 80
EXPOSE 80

# Запускаем Nginx в foreground
CMD ["nginx", "-g", "daemon off;"]