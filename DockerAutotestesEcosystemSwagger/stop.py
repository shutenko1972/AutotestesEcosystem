#!/usr/bin/env python3
"""Скрипт для остановки контейнеров"""

import subprocess

print("🛑 Останавливаю контейнеры...")
result = subprocess.run(["docker-compose", "down"], capture_output=True, text=True)

if result.returncode == 0:
    print("✅ Контейнеры остановлены")
else:
    print(f"❌ Ошибка: {result.stderr}")