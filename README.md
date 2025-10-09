# APISegaAI 🚀

**APISegaAI** — RESTful API для автоматизации генерации, управления и экспорта тест-кейсов с использованием моделей машинного обучения (LLM, таких как Qwen3-32b, Gemma 3-27b) и интеграции с Zephyr Scale. Проект предназначен для упрощения создания тест-кейсов, проверки их соответствия требованиям, управления тестовыми данными (datapool) и экспорта результатов в Excel, CSV и Zephyr Scale.

## 📋 Описание

Проект реализует функциональность для:
- 🧠 Генерации тест-кейсов на основе входных данных (текста, файлов, URL) с использованием LLM.
- ✅ Проверки тест-кейсов на соответствие требованиям с рекомендациями для улучшения.
- 📊 Управления тестовыми данными (datapool), включая автоматическую генерацию и загрузку пользовательских данных.
- 📤 Экспорта тест-кейсов и данных в форматы Excel, CSV, Zephyr Scale и Java-код (JUnit/Selenium).
- 📜 Ведения истории операций над тест-кейсами.

## 🏗 Архитектура

Проект построен на основе многослойной архитектуры:

- **APISegaAI.Domain** 🗂:
  - Содержит доменные сущности (`TestCase`, `TestStep`, `DataPool`, `DataPoolItem`, `InputData`, `HistoryEntry`) и перечисления (`TestCaseStatus`, `DataPoolSource`).
  - Определяет бизнес-логику и контракты данных.

- **APISegaAI.DAL** 🗄:
  - Слой доступа к данным, использующий Entity Framework Core и PostgreSQL.
  - Реализует паттерн Unit of Work и Repository для работы с сущностями.
  - Зависимости: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design`.

- **APISegaAI.Service** ⚙:
  - Содержит бизнес-логику для генерации, валидации, экспорта тест-кейсов и управления данными.
  - Интегрируется с LLM через REST API и обрабатывает файлы с помощью `ClosedXML` и `HtmlAgilityPack`.
  - Зависимости: `ClosedXML`, `HtmlAgilityPack`, `Microsoft.Extensions.Http`.

- **APISegaAI (основной проект)** 🌐:
  - REST API на ASP.NET Core 7.0 с поддержкой OpenAPI/Swagger.
  - Контроллеры для управления тест-кейсами, тестовыми данными, экспортом и историей.
  - Зависимости: `Swashbuckle.AspNetCore`, `Microsoft.Extensions.Http.Polly`.

## 🛠 Требования

- **.NET SDK**: 7.0 🖥
- **PostgreSQL**: 13+ 🗄
- **LLM API**: Доступ к API моделей (например, Qwen3-32b, Gemma 3-27b) 🌐
- **Zephyr Scale**: Доступ к REST API Zephyr Scale для экспорта тест-кейсов 📤
- **Docker** (опционально): Для запуска в контейнерах 🐳

## 🔧 Установка и настройка

### 1. Клонирование репозитория 📥
```bash
git clone https://github.com/your-repo/APISegaAI.git
cd APISegaAI
```

### 2. Настройка окружения ⚙
1. Создайте файл `appsettings.json` в корне основного проекта:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=segaai;Username=your_user;Password=your_password"
  },
  "LlmSettings": {
    "BaseUrl": "https://your-llm-api-endpoint",
    "CertificatePath": "Certificates/your-certificate.cer",
    "MaxTokens": "10000",
    "Temperature": "0.7",
    "TimeoutSeconds": "300",
    "ModelEndpoints": {
        "name-model-ai": "openai/chat/completions",
        "name-model-ai": "openai/chat/completions",
        "name-model-ai": "openai/chat/completions",
        "name-model-ai": "openai/chat/completions"
      }
  },
  "ZephyrSettings": {
    "BaseUrl": "https://api.zephyrscale.com/v2",
    "ApiKey": "your-zephyr-api-key",
    "ProjectKey": "TEST"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```
2. Убедитесь, что PostgreSQL запущен и база данных создана:
```bash
psql -U your_user -c "CREATE DATABASE segaai;"
```

3. Примените миграции для создания таблиц:
```bash
cd APISegaAI
dotnet ef migrations add InitialCreate --project ../APISegaAI.DAL
dotnet ef database update --project ../APISegaAI.DAL
```

### 3. Установка зависимостей 📦
```bash
dotnet restore
```

### 4. Запуск приложения ▶
```bash
cd APISegaAI
dotnet run
```
API будет доступно по адресу `https://localhost:5001` (или другой порт, указанный в `launchSettings.json`).

### 5. Доступ к Swagger 📖
Откройте в браузере: `https://localhost:5001/swagger`

## 📦 Зависимости

### Основной проект (APISegaAI)
- **Microsoft.AspNetCore.OpenApi**: 7.0.20 📜
- **Microsoft.EntityFrameworkCore.Design**: 7.0.0 🗄
- **Microsoft.Extensions.Http.Polly**: 7.0.0 🌐
- **Npgsql.EntityFrameworkCore.PostgreSQL**: 7.0.0 🗄
- **Swashbuckle.AspNetCore**: 6.5.0 📖
- **Swashbuckle.AspNetCore.Annotations**: 6.5.0 📖

### APISegaAI.DAL
- **Microsoft.EntityFrameworkCore.Design**: 7.0.0 🗄
- **Microsoft.Extensions.Configuration.Json**: 7.0.0 ⚙
- **Npgsql.EntityFrameworkCore.PostgreSQL**: 7.0.0 🗄

### APISegaAI.Service
- **ClosedXML**: 0.96.0 📑
- **HtmlAgilityPack**: 1.11.52 🌐
- **Microsoft.AspNetCore.Http.Abstractions**: 2.2.0 🌐
- **Microsoft.Extensions.Http**: 7.0.0 🌐
- **Microsoft.Extensions.Logging.Abstractions**: 7.0.0 📝

### APISegaAI.Domain
- Нет внешних зависимостей ✅

## 🌟 Основные возможности

### Генерация тест-кейсов 🧠
- Эндпоинт: `POST /api/v1/test-cases/generate`
- Генерирует тест-кейс и JUnit/Selenium-код на основе входных данных (текст, файлы, URL) с использованием LLM.
- Пример запроса:
```json
{
  "inputData": "Проверить форму логина на сайте https://example.com/login",
  "llmModel": "qwen3-32b-awq"
}
```

### Управление тестовыми данными (datapool) 📊
- Эндпоинты:
  - `POST /api/v1/datapool/generate` — Генерация тестовых данных через LLM.
  - `POST /api/v1/datapool/upload` — Загрузка пользовательских данных (JSON/CSV).
  - `GET /api/v1/datapool/{id}` — Получение datapool по ID.
  - `GET /api/v1/datapool/by-testcase/{testCaseId}` — Получение datapool по ID тест-кейса.

### Экспорт 📤
- Эндпоинты:
  - `GET /api/v1/export/testcase/{testCaseId}/excel` — Экспорт тест-кейса в Excel.
  - `GET /api/v1/export/datapool/{dataPoolId}/csv` — Экспорт тестовых данных в CSV.
  - `POST /api/v1/export/testcase/{testCaseId}/zephyr` — Экспорт в Zephyr Scale.
  - `GET /api/v1/export/testcase/{testCaseId}/code` — Экспорт JUnit/Selenium-кода.

### История операций 📜
- Эндпоинт: `GET /api/v1/history/by-testcase/{testCaseId}`
- Возвращает историю операций для тест-кейса (генерация, обновление, экспорт).

## 💻 Пример использования

### Генерация тест-кейса
```bash
curl -X POST "https://localhost:5001/api/v1/test-cases/generate" \
  -H "Content-Type: application/json" \
  -d '{
    "inputData": "Проверить форму логина на сайте https://example.com/login",
    "llmModel": "qwen3-32b-awq"
  }'
```
Ответ:
```json
{
  "testCase": {
    "id": "guid-123",
    "number": "TC-001",
    "name": "Тест формы логина",
    "steps": [
      {
        "stepNumber": 1,
        "action": "Ввести имя пользователя 'testuser'",
        "expectedResult": "Поле заполнено"
      }
    ],
    "testCode": "@Test\npublic void testLoginForm() {\n  driver.get(\"https://example.com/login\");\n  driver.findElement(By.id(\"username\")).sendKeys(\"testuser\");\n  driver.quit();\n}"
  },
  "recommendation": null
}
```

### Экспорт в Excel
```bash
curl -X GET "https://localhost:5001/api/v1/export/testcase/guid-123/excel" \
  -o "TestCase_guid-123.xlsx"
```

## 🛠 Разработка

### Миграции базы данных
Для добавления новой миграции:
```bash
cd APISegaAI
dotnet ef migrations add <MigrationName> --project ../APISegaAI.DAL
dotnet ef database update --project ../APISegaAI.DAL
```

### Настройка LLM API
Убедитесь, что в `appsettings.json` указан корректный `LlmSettings:BaseUrl` и параметры (`MaxTokens`, `Temperature`).

### Настройка Zephyr Scale
Добавьте `ZephyrSettings:ApiKey` и `ZephyrSettings:ProjectKey` в `appsettings.json` для интеграции с Zephyr Scale.

## 🧪 Тестирование
- Используйте Swagger UI (`/swagger`) для тестирования эндпоинтов.
- Для юнит-тестов добавьте проект с использованием `Moq` и `Microsoft.EntityFrameworkCore.InMemory`.