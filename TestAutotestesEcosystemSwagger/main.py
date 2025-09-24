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
    title="🔐 Service API",
    description="<h3><strong>API для аутентификации и проверки функций</strong></h3><br><br>"
                "<div style='background-color: #f8f9fa; padding: 10px; border-radius: 5px; border-left: 4px solid #3498db;'>"
                "<h3 style='color: #2c3e50; margin-top: 0;'>Функциональность:</h3>"
                "<ul style='color: #7f8c8d;'>"
                "<li>Аутентификация пользователей</li>"
                "<li>Безопасный выход из системы</li>"
                "<li>Проверка работаспособности функций</li>"
                "</ul>"
                "</div>",
    version="1.0.0"
)

# Хранилище сессий (в памяти)
active_sessions: Dict[str, dict] = {}

# Модели данных
class LoginRequest(BaseModel):
    login: str
    password: str

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

# Функция для проверки сессии
def get_session(token: str = Form(...)):
    """Проверяет валидность сессии"""
    if token not in active_sessions:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Сессия недействительна или истекла"
        )
    return active_sessions[token]

# ВСЕ три эндпоинта в одной группе: Authentication
@app.post(
    "/api/auth/login",
    response_model=LoginResponse,
    responses={
        400: {"model": ErrorResponse, "description": "Неверные параметры запроса"},
        401: {"model": ErrorResponse, "description": "Неверные учетные данные"},
        500: {"model": ErrorResponse, "description": "Внутренняя ошибка сервера"}
    },
    summary="Аутентификация пользователя",
    description="Проверяет логин и пароль пользователя и создает сессию длительностью 1 час",
    tags=["Authentication"]
)
async def login(
    login: str = Form(..., description="Логин пользователя", example="v_shutenko"),
    password: str = Form(..., description="Пароль пользователя", example="8nEThznM")
):
    try:
        logger.info(f"Login attempt for user: {login}")

        # Валидация
        if not login or not password:
            logger.warning("Validation failed: Login and password are required")
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Логин и пароль обязательны"
            )

        # Проверка учетных данных
        if login != "v_shutenko" or password != "8nEThznM":
            logger.warning(f"Invalid credentials for user: {login}")
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Неверные учетные данные"
            )

        # Создание сессии
        session_token = str(uuid.uuid4())
        active_sessions[session_token] = {
            "user_login": login,
            "created_at": datetime.now(),
            "expires_at": datetime.now() + timedelta(hours=1)
        }

        logger.info(f"User {login} authenticated successfully. Session created.")

        return LoginResponse(
            message="Успешная аутентификация",
            redirectUrl="https://ai-ecosystem-test.janusww.com:9999/request/model.html",
            sessionToken=session_token
        )

    except HTTPException:
        raise
    except Exception as ex:
        logger.error(f"Error during login for user: {login} - {ex}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Внутренняя ошибка сервера"
        )

@app.post(
    "/api/auth/logout",
    response_model=LogoutResponse,
    responses={
        401: {"model": ErrorResponse, "description": "Недействительная сессия"},
        500: {"model": ErrorResponse, "description": "Ошибка при выходе из системы"}
    },
    summary="Выход пользователя из системы",
    description="Завершает сессию пользователя и удаляет токен из активных сессий",
    tags=["Authentication"]
)
async def logout(session_token: str = Form(..., description="Токен сессии для выхода", example="uuid-токен")):
    try:
        # Проверяем существование сессии
        if session_token not in active_sessions:
            logger.warning("Logout attempt with invalid session token")
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Сессия недействительна"
            )

        # Удаляем сессию
        user_login = active_sessions[session_token]["user_login"]
        del active_sessions[session_token]
        
        logger.info(f"User {user_login} logged out successfully")
        return LogoutResponse(message="Успешный выход из системы")
        
    except HTTPException:
        raise
    except Exception as ex:
        logger.error(f"Error during logout - {ex}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Ошибка при выходе из системы"
        )

@app.post(
    "/api/auth/check-session",
    response_model=SessionInfoResponse,
    responses={
        401: {"model": ErrorResponse, "description": "Недействительная сессия"},
        500: {"model": ErrorResponse, "description": "Ошибка при проверке сессии"}
    },
    summary="Проверка сессии",
    description="Проверяет валидность сессии пользователя и возвращает информацию о ней",
    tags=["Authentication"]
)
async def check_session(session_token: str = Form(..., description="Токен сессии для проверки", example="uuid-токен")):
    try:
        if session_token not in active_sessions:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Сессия недействительна"
            )
        
        session = active_sessions[session_token]
        return SessionInfoResponse(
            valid=True,
            user_login=session["user_login"],
            expires_at=session["expires_at"]
        )
    except HTTPException:
        raise
    except Exception as ex:
        logger.error(f"Error during session check - {ex}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Ошибка при проверке сессии"
        )

# Дополнительный эндпоинт для получения информации о сервере
@app.get("/", include_in_schema=False)
async def root():
    return {
       "message": "Service API",
       "version": "1.0.0",
       "docs": "/docs",
       "endpoints": {
            "login": "/api/auth/login",
            "logout": "/api/auth/logout", 
            "check_session": "/api/auth/check-session"
       }
    }

# Запуск приложения
if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)