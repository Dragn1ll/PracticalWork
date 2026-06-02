# Общие сведения
### Наименование сервиса
PracticalWork.Library

### Назначение
Получение опыта в основах ООП и Docker.
Разработка системы управления библиотекой.

### Исполняемые модули
1. PracticalWork.Library.Web - ASP.NET 8 WebApi
2. PracticalWork.Library.Data.PostgreSql.Migrator - запуск миграций

### Интеграции
1. База данных - PostgreSQL
2. Распределенный кэш - Redis
3. Хранение файлов - MinIO

### Инструменты разработки
1. Rider, Visual Studio 2022 или VS Code
2. PostgreSQL pgAdmin или DBeaver
3. Redis Insight (опционально)

# Развертывание и конфигурирование сервиса
### Развертывание
- Развертывание кода сервиса: src.PracticalWork.Library.Web.Dockerfile
- Развертывание интеграций: docker-compose.yaml
- Импорт начальных данных: ...