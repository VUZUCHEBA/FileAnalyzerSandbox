
# 🔒 Анализатор файлов в изолированной среде (Sandbox)

Веб-приложение для безопасного анализа файлов в изолированной среде. Позволяет загружать файлы и проверять их на наличие вредоносного кода, вирусов и других угроз.

## Содержание

- [Технологии](#технологии)
- [Архитектура проекта](#архитектура-проекта)
- [Требования](#требования)
- [Установка и запуск](#установка-и-запуск)
- [Docker](#docker)
- [API Endpoints](#api-endpoints)
- [Структура проекта](#структура-проекта)
- [Конфигурация](#Конфигурация)
- [Автор](#Автор)
- [Функциональность](#Функциональность)



## Технологии
- .NET 8.0
- Blazor Server
- Entity Framework Core 8.0 (Code First)
- SQLite (разработка)
- Bootstrap 5.3
- Docker / Docker Compose

## Архитектура проекта
Проект построен на многослойной архитектуре:
Domain — сущности и интерфейсы репозиториев 
Application — бизнес-логика, сервисы, DTO
Infrastructure — реализация репозиториев, DbContext, миграции
Web — Blazor компоненты, страницы, точка входа

Связи между сущностями в базе данных:
- Один пользователь может иметь много анализов (1:N)
- Один анализ может иметь много логов (1:N)

## Требования

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (опционально)
- [Git](https://git-scm.com/)
- Visual Studio 2022 или VS Code


## Установка и запуск

### Способ 1: Локальный запуск (без Docker)

```bash
# 1. Клонировать репозиторий
git clone https://github.com/VUZUCHEBA/FileAnalyzerSandbox.git
cd FileAnalyzerSandbox

# 2. Восстановить пакеты
dotnet restore

# 3. Собрать проект
dotnet build

# 4. Применить миграции БД
dotnet ef migrations add InitialCreate --project src/FileAnalyzerSandbox.Infrastructure --startup-project src/FileAnalyzerSandbox.Web
dotnet ef database update --project src/FileAnalyzerSandbox.Infrastructure --startup-project src/FileAnalyzerSandbox.Web

# 5. Запустить приложение
cd src/FileAnalyzerSandbox.Web
dotnet run

# 6. Открыть браузер
# http://localhost:5000
```

### Способ 2: Запуск через Visual Studio 2022

1. Открыть решение `FileAnalyzerSandbox.sln`
2. Выбрать проект `FileAnalyzerSandbox.Web` как стартовый
3. Нажать `F5` для запуска

### Способ 3: Запуск в Docker

```bash
# 1. Собрать и запустить контейнеры
docker-compose up --build

# 2. Открыть браузер
# http://localhost:5000
```

## 🐳 Docker

### Структура Docker файлов

```
FileAnalyzerSandbox/
├── Dockerfile                 # Для сборки образа Web приложения
├── docker-compose.yml         # Оркестрация контейнеров
├── docker-compose.override.yml # Переопределение для разработки
└── .dockerignore              # Исключения для Docker
```

### Docker команды

```bash
# Сборка образа
docker build -t fileanalyzer-sandbox -f src/FileAnalyzerSandbox.Web/Dockerfile .

# Запуск контейнера
docker run -p 5000:8080 fileanalyzer-sandbox

# Запуск через docker-compose
docker-compose up

# Остановка контейнеров
docker-compose down

# Просмотр логов
docker-compose logs -f
```

## API Endpoints

| Метод | Endpoint | Описание |
|-------|----------|----------|
| GET | `/api/status` | Проверка статуса приложения |
| POST | `/api/analyze/start/{analysisId}` | Запуск анализа файла |
| GET | `/health` | Health check |

## 📁 Структура проекта

```
FileAnalyzerSandbox/
├── src/
│   ├── FileAnalyzerSandbox.Domain/        # Domain Layer
│   │   ├── Entities/                      # Сущности
│   │   └── Interfaces/                    # Интерфейсы
│   │   
│   │
│   ├── FileAnalyzerSandbox.Infrastructure/ # Infrastructure Layer
│   │   ├── Data/                          # DbContext
│   │   ├── Migrations/                    # EF Core миграции
│   │   └── Repositories/                  # Репозитории
│   │
│   ├── FileAnalyzerSandbox.Application/   # Application Layer
│   │   ├── Services/                      # Сервисы
│   │   └─── DTOs/                          # Data Transfer Objects
│   │   
│   │
│   ├── FileAnalyzerSandbox.Web/           # Presentation Layer
│   │   ├── Components/
│   │   │   ├── Pages/                     # Razor страницы
│   │   │   │   ├── Home.razor
│   │   │   │   ├── FileUpload.razor
│   │   │   │   ├── AnalysisHistory.razor
│   │   │   │   └── AnalysisDetails.razor
│   │   │   └── Layout/                    # Layout компоненты
│   │   ├── wwwroot/                       # Статические файлы
│   │   ├── appsettings.json               # Конфигурация
│   │   └── Program.cs                     # Точка входа
│   │
│   └── FileAnalyzerSandbox.Sandbox/       # Sandbox сервис
│
├── tests/
│   └── FileAnalyzerSandbox.Tests/         # Unit тесты
│
├── docker-compose.yml
├── Dockerfile
└── README.md
```

## Конфигурация

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=fileanalyzer.db"
  },
  "Sandbox": {
    "SandboxPath": "sandbox",
    "MaxFileSizeMB": 20,
    "AnalysisTimeoutSeconds": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## Тестирование

```bash
# Запуск всех тестов
dotnet test

# Запуск с покрытием
dotnet test --collect:"XPlat Code Coverage"
```

## Миграции базы данных

```bash
# Добавление миграции
dotnet ef migrations add MigrationName --project src/FileAnalyzerSandbox.Infrastructure --startup-project src/FileAnalyzerSandbox.Web

# Применение миграции
dotnet ef database update --project src/FileAnalyzerSandbox.Infrastructure --startup-project src/FileAnalyzerSandbox.Web

# Удаление последней миграции
dotnet ef migrations remove --project src/FileAnalyzerSandbox.Infrastructure --startup-project src/FileAnalyzerSandbox.Web

# Просмотр списка миграций
dotnet ef migrations list --project src/FileAnalyzerSandbox.Infrastructure --startup-project src/FileAnalyzerSandbox.Web
```

## Автор

Nikolai — [GitHub](https://github.com/sex95)

## Лицензия

Этот проект разработан в рамках курсовой работы по дисциплине «Кроссплатформенная среда исполнения программного обеспечения».

## Функциональность

- Загрузка файлов (до 20 МБ)
- Анализ файлов в изолированной среде
- Обнаружение вредоносных сигнатур
- Проверка на маскировку EXE файлов
- История анализов
- Детальные отчеты
- Docker контейнеризация
- Адаптивный дизайн

## Известные проблемы

- Файлы размером более 20 МБ не принимаются
- Анализ работает только для текстовых и исполняемых файлов

## Поддержка

При возникновении проблем:
1. Проверьте логи в консоли
2. Убедитесь что установлен .NET 8.0
3. Очистите кэш браузера
```
