## PracticalWork — система управления библиотекой (два микросервиса)

### Назначение
Получение опыта в ООП, микросервисной архитектуре, Docker.  
Разработка системы: учёт книг, читателей, выдачи, логирование событий, генерация CSV‑отчётов.

### Исполняемые модули
1. **Library.Web** — ASP.NET Core (порт 5251): управление книгами, читателями, выдачей, фоновые задачи Hangfire.
2. **Reports.Web** — ASP.NET Core (порт 5093): логирование, отчёты.
3. **Library.Data.PostgreSql.Migrator** — миграции БД Library.
4. **Reports.Data.PostgreSql.Migrator** — миграции БД Reports.

### Интеграции
- **PostgreSQL** (Library:5433, Reports:5434)
- **Redis** (6379) — кэш
- **MinIO** (9000/9001) — обложки книг и CSV
- **RabbitMQ** (5672/15672) — брокер событий
- **Hangfire** (через PostgreSQL) — планировщик задач
- **smtp4dev** (25/5000) — перехват email
- **MailKit** — отправка SMTP

### Инструменты разработки
- Rider / VS 2022 / VS Code
- pgAdmin (5050) или DBeaver
- Redis Commander (8081)
- RabbitMQ Management (15672)
- MinIO Console (9001)
- smtp4dev Web (5000)
- Hangfire Dashboard (http://localhost:5251/hangfire)

## Развертывание и конфигурирование

### Быстрый старт

```bash
# 1. Запустить инфраструктуру
docker compose up -d

# 2. Применить миграции
cd Library/Library.Data.PostgreSql.Migrator && dotnet run
cd ../../Reports/Reports.Data.PostgreSql.Migrator && dotnet run

# 3. Запустить сервисы (в отдельных терминалах)
cd Library/Library.Web && dotnet run   # http://localhost:5251
cd Reports/Reports.Web && dotnet run   # http://localhost:5093
```

**Порты инфраструктуры (основные):**
- PostgreSQL: 5433 (library), 5434 (reports)
- Redis: 6379
- MinIO API:9000, Console:9001
- RabbitMQ AMQP:5672, Management:15672
- smtp4dev SMTP:25, Web:5000

**Проверка:**
- Library Swagger → http://localhost:5251/swagger
- Reports Swagger → http://localhost:5093/swagger
- RabbitMQ → http://localhost:15672 (user/pass)
- MinIO Console → http://localhost:9001 (minioadmin/minioadmin)

### Конфигурация (кратко)

**Library (`appsettings.json`)**:
- `App:DbConnectionString` — порт 5433
- `App:Redis:RedisCacheConnection` — localhost:6379
- `App:Minio` — Endpoint, AccessKey, SecretKey, BucketName
- `RabbitMq` — HostName, Port, UserName, Password
- `ReportsService:BaseUrl` — http://localhost:5093
- `EmailSettings` — SmtpServer=localhost, порт 25
- `JobSettings:Jobs` — cron‑расписания задач

**Reports (`appsettings.json`)**:
- `App:DbConnectionString` — порт 5434
- `App:Minio:BucketName` — "reports"
- `RabbitMq` — параметры подключения
- `QueueNames` — имена очередей событий

**Смена окружения**:
```bash
ASPNETCORE_ENVIRONMENT=Production dotnet run
```

## API (основные)

Префикс `/api/v1`

### Library

| Метод | Путь | Описание |
|-------|------|----------|
| `POST` | `/books` | Создать книгу |
| `GET` | `/books` | Список книг (фильтры, пагинация) |
| `POST` | `/library/borrow` | Выдать книгу |
| `POST` | `/library/return` | Вернуть книгу |
| `POST` | `/readers` | Создать читателя |
| `GET` | `/reports/activity` | Логи активности (прокси) |
| `POST` | `/reports` | Инициировать отчёт |
| `GET` | `/reports` | Список готовых отчётов |

### Reports

| Метод | Путь | Описание |
|-------|------|----------|
| `GET` | `/reports/activity` | Логи с фильтрами (dateFrom, dateTo, eventType) |
| `GET` | `/reports/activity/statistic` | Статистика за период |
| `POST` | `/reports` | Создать отчёт (статус InProgress) |
| `GET` | `/reports` | Готовые отчёты (статус Generated) |
| `GET` | `/reports/{name}/download` | Presigned‑ссылка на CSV |

## Фоновые задачи (Hangfire в Library)

| Задача | Расписание (UTC) | Действие |
|--------|------------------|----------|
| `ReturnReminderJob` | ежедневно 06:00 | Напоминание о возврате книг |
| `WeeklyReportJob` | каждый пн 05:00 | Недельный отчёт администраторам |
| `ArchiveOldBooksJob` | 1‑го числа 00:00 | Архивация книг, не выдававшихся > N лет |

## Разработка

### Добавление миграций EF Core
```bash
cd Library/Library.Data.PostgreSql
dotnet ef migrations add <MigrationName> --startup-project ../Library.Data.PostgreSql.Migrator
# аналогично для Reports
```

### Сборка
```bash
cd Library && dotnet build Library.sln
cd Reports && dotnet build Reports.sln
```