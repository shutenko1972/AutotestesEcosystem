from fastapi import FastAPI, Form, HTTPException, status, Depends
from pydantic import BaseModel
from typing import Optional, Dict
import logging
import uuid
from datetime import datetime, timedelta

# Настройка логирования
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Создание приложения с кастомной конфигурацией
app = FastAPI(
    title="🔐 Ai ecosystem test API",
    description="<h3><strong>API для аутентификации и проверки функций</strong></h3>"
                "<div style='background-color: #f8f9fa; padding: 10px; border-radius: 5px; border-left: 4px solid #3498db;'>"
                "<h3 style='color: #2c3e50; margin-top: 0;'>Функциональность:</h3>"
                "<ul style='color: #7f8c8d;'>"
                "<strong><li>Аутентификация (Auth)</li></strong>"
                "<li>Вход пользователя в систему</li>"
                "<li>Выход пользователя из системы</li>"
                "<li>Проверка валидности сессии</li><br>"
                "<strong><li>Чат (Chat)</li></strong>"
                "<li>Отправка сообщения в AI-чат</li>"
                "<li>Очистка истории чата</li>"
                "<li>Копирование текста ответа</li><br>"
                "<strong><li>Настройки модели (Settings)</li></strong>"
                "<li>Установка параметра температуры AI-модели</li>"
                "<li>Установка параметра Top-P AI-модели</li><br>"
                "<strong><li>Системные (System)</li></strong>"
                "<li>Проверка работоспособности сервера </li>"
                "<li>Получение информации о сервере</li>"
                "</ul>"
                "</div>",
    version="1.0.1"
)

# Хранилище сессий (в памяти)
active_sessions: Dict[str, dict] = {}

# Модели данных
class LoginResponse(BaseModel):
    message: str
    redirectUrl: str
    sessionToken: str

class ErrorResponse(BaseModel):
    error: str

class LogoutResponse(BaseModel):
    message: str

class SessionInfoResponse(BaseModel):
    valid: bool
    user_login: str
    expires_at: datetime

class ChatResponse(BaseModel):
    answer: str

class TemperatureResponse(BaseModel):
    value: int

class TopPResponse(BaseModel):
    value: int

class ClearResponse(BaseModel):
    message: str

class CopyResponse(BaseModel):
    message: str

class HealthResponse(BaseModel):
    status: str

# Функция для проверки сессии
def get_session(token: str = Form(...)):
    if token not in active_sessions:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid session"
        )
    return active_sessions[token]

# Аутентификация
@app.post(
    "/api/auth/login",
    response_model=LoginResponse,
    tags=["Auth"]
)
async def login(
    login: str = Form(...),
    password: str = Form(...)
):
    if login != "v_shutenko" or password != "8nEThznM":
        raise HTTPException(status_code=401, detail="Invalid credentials")
    
    session_token = str(uuid.uuid4())
    active_sessions[session_token] = {
        "user_login": login,
        "expires_at": datetime.now() + timedelta(hours=1)
    }
    
    return LoginResponse(
        message="Success",
        redirectUrl="/request/model.html",
        sessionToken=session_token
    )

@app.post(
    "/api/auth/logout",
    response_model=LogoutResponse,
    tags=["Auth"]
)
async def logout(session_token: str = Form(...)):
    if session_token in active_sessions:
        del active_sessions[session_token]
    return LogoutResponse(message="Logged out")

@app.post(
    "/api/auth/check-session",
    response_model=SessionInfoResponse,
    tags=["Auth"]
)
async def check_session(session_token: str = Form(...)):
    session = get_session(session_token)
    return SessionInfoResponse(
        valid=True,
        user_login=session["user_login"],
        expires_at=session["expires_at"]
    )

# Функции чата
@app.post(
    "/api/chat/send",
    response_model=ChatResponse,
    tags=["Chat"]
)
async def chat_send(
    message: str = Form(...),
    session_token: str = Form(...)
):
    get_session(session_token)
    
    # Простой ответ
    if "привет" in message.lower():
        response = "Привет! Чем могу помочь?"
    elif "погода" in message.lower():
        response = "Погода хорошая"
    elif "время" in message.lower():
        response = f"Сейчас {datetime.now().strftime('%H:%M')}"
    else:
        response = "Получил ваш запрос"
    
    return ChatResponse(answer=response)

@app.post(
    "/api/chat/clear",
    response_model=ClearResponse,
    tags=["Chat"]
)
async def chat_clear(session_token: str = Form(...)):
    get_session(session_token)
    return ClearResponse(message="Chat cleared")

@app.post(
    "/api/chat/copy",
    response_model=CopyResponse,
    tags=["Chat"]
)
async def chat_copy(
    text: str = Form(...),
    session_token: str = Form(...)
):
    get_session(session_token)
    return CopyResponse(message="Text copied")

# Настройки модели
@app.post(
    "/api/settings/temperature",
    response_model=TemperatureResponse,
    tags=["Settings"]
)
async def set_temperature(
    value: int = Form(..., ge=0, le=200),
    session_token: str = Form(...)
):
    get_session(session_token)
    return TemperatureResponse(value=value)

@app.post(
    "/api/settings/topp",
    response_model=TopPResponse,
    tags=["Settings"]
)
async def set_topp(
    value: int = Form(..., ge=0, le=100),
    session_token: str = Form(...)
):
    get_session(session_token)
    return TopPResponse(value=value)

# Системные функции
@app.get(
    "/api/health",
    response_model=HealthResponse,
    tags=["System"]
)
async def health_check():
    return HealthResponse(status="OK")

@app.get(
    "/api/info",
    tags=["System"]
)
async def server_info():
    return {
        "name": "AI Service",
        "version": "1.0.0"
    }

# Корневой эндпоинт
@app.get("/")
async def root():
    return {"message": "Service API"}

# Запуск приложения
if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)