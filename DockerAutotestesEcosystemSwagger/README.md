<!-- Начало

Личный кабинет
https://ai-ecosystem-test.janusww.com:9999/auth/login.html
v_shutenko 
8nEThznM

Узнать структуру приложения в корне проекта:
tree /F

Отлично! Вот правильные команды для запуска через терминал:

## 1. **Запуск в режиме разработки (без Docker):**

```bash
# Перейдите в папку проекта
cd C:\Users\shute\source\AutotestesEcosystem\DockerAutotestesEcosystemSwagger

# Запустите приложение
dotnet run
```

Приложение будет доступно:
- **http://localhost:5000**
- **https://localhost:7000**
- Swagger: **http://localhost:5000/swagger**

## 2. **Запуск через Docker (ручная сборка):**

```bash
# В папке проекта

# Соберите образ
docker build -t auth-api .

# Запустите контейнер
docker run -p 5000:80 -e ASPNETCORE_ENVIRONMENT=Development auth-api
```

Или для HTTPS:
```bash
docker run -p 5000:80 -p 7000:443 -e ASPNETCORE_ENVIRONMENT=Development auth-api
```

## 3. **Полезные команды Docker:**

```bash
# Посмотреть запущенные контейнеры
docker ps

# Остановить контейнер
docker stop <container_id>

# Посмотреть логи контейнера
docker logs <container_id>

# Удалить все остановленные контейнеры
docker container prune

# Удалить все неиспользуемые образы
docker image prune -a
```

## 4. **Тестовые запросы через curl:**

```bash
# Проверить главную страницу
curl http://localhost:5000/

# Тестовый login запрос
curl -X POST "http://localhost:5000/api/auth/login" ^
  -H "Content-Type: application/x-www-form-urlencoded" ^
  -d "login=v_shutenko&password=8nEThznM"

# Для PowerShell используйте ` вместо ^
curl -X POST "http://localhost:5000/api/auth/login" `
  -H "Content-Type: application/x-www-form-urlencoded" `
  -d "login=v_shutenko&password=8nEThznM"
```

## 5. **Запуск с разными окружениями:**

```bash
# Production
docker run -p 5000:80 -e ASPNETCORE_ENVIRONMENT=Production auth-api

# С изменёнными переменными
docker run -p 5000:80 ^
  -e ASPNETCORE_ENVIRONMENT=Development ^
  -e ASPNETCORE_URLS=http://+:80 ^
  auth-api
```

## 6. **Фоновый режим (detached):**

```bash
# Запуск в фоне
docker run -d -p 5000:80 --name my-auth-api auth-api

# Остановить
docker stop my-auth-api

# Запустить снова
docker start my-auth-api
```

## 7. **Быстрый старт (самый простой способ):**

```bash
# Всего 2 команды:
docker build -t auth-api .
docker run -p 5000:80 auth-api
```

**После этого открывайте: http://localhost:5000/swagger**

Какой способ запуска вас интересует больше?

Конец -->