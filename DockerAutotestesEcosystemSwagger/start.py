#!/usr/bin/env python3
"""
Скрипт для запуска Docker контейнеров и открытия Swagger в браузере
"""

import time
import webbrowser
import subprocess
import sys

def check_docker():
    """Проверка установки Docker"""
    try:
        subprocess.run(["docker", "--version"], check=True, capture_output=True)
        return True
    except:
        print("❌ Docker не установлен или не запущен")
        return False

def start_containers():
    """Запуск контейнеров через docker-compose"""
    print("🚀 Запуск контейнеров...")
    
    # Останавливаем предыдущие контейнеры
    subprocess.run(["docker-compose", "down"], capture_output=True)
    
    # Запускаем
    result = subprocess.run(
        ["docker-compose", "up", "-d"],
        capture_output=True,
        text=True
    )
    
    if result.returncode != 0:
        print(f"❌ Ошибка: {result.stderr}")
        return False
    
    print("✅ Контейнеры запущены")
    return True

def wait_for_services():
    """Ожидание готовности сервисов"""
    print("⏳ Ожидание запуска сервисов...")
    time.sleep(5)  # Ждем 5 секунд
    print("✅ Сервисы готовы")

def open_browser():
    """Открытие браузера с Swagger UI"""
    urls = [
        "http://localhost:8080",  # Swagger UI
        "http://localhost:8000/docs"  # FastAPI Swagger
    ]
    
    for url in urls:
        print(f"🌐 Открываю {url}")
        webbrowser.open(url)

def main():
    """Основная функция"""
    if not check_docker():
        sys.exit(1)
    
    if not start_containers():
        sys.exit(1)
    
    wait_for_services()
    open_browser()
    
    print("\n🎯 Сервисы запущены:")
    print("   • FastAPI API: http://localhost:8000")
    print("   • Swagger UI: http://localhost:8080")
    print("   • FastAPI Docs: http://localhost:8000/docs")
    print("\n⏹️  Для остановки нажмите Ctrl+C")

if __name__ == "__main__":
    try:
        main()
        # Держим скрипт активным
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        print("\n🛑 Остановка контейнеров...")
        subprocess.run(["docker-compose", "down"])
        print("✅ Контейнеры остановлены")