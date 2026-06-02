## Критические

**1. Домен тянет инфраструктуру**

В проекте `PracticalWork.Library` лежат `RabbitMqProducer.cs` и `EmailTemplateService.cs`. Домен ссылается на `RabbitMQ.Client`, `RazorLight` и `Microsoft.AspNetCore.Http` напрямую. Это всё должно быть в инфраструктурных проектах, а домен должен знать только об интерфейсах.

- `src/PracticalWork.Library/Services/RabbitMqProducer.cs` — реализация брокера в домене
- `src/PracticalWork.Library/Services/EmailTemplateService.cs` — рендеринг шаблонов через RazorLight в домене
- `src/PracticalWork.Library/Abstractions/Services/IBookService.cs` — метод принимает `IFormFile`

---

## Серьёзные

**2. Пагинация в памяти, BookRepository.GetBooks**

Сначала `ToListAsync()` загружает все книги из базы, потом `Skip`/`Take` режет список в памяти. При большом числе записей убьёт производительность. Нужно делать пагинацию на уровне SQL.

**3. RedisService хранит ключи в List\<string\>**

`RemoveByPrefixAsync` ищет ключи по локальному `List<string> _keys`. После перезапуска список пуст — очистка кэша не работает. Плюс список растёт бесконечно.

- `src/PracticalWork.Library.Cache.Redis/RedisService.cs`

**4. Синхронный `Any()` в async-методе — ReaderRepository**

`CreateReader` объявлен как `async`, но `_appDbContext.Readers.Any(...)` — синхронный вызов. Блокирует поток. Надо `AnyAsync()`.

- `src/PracticalWork.Library.Data.PostgreSql/Repositories/ReaderRepository.cs`

**5. Утечка scope — ReportGenerateConsumer**

`_scopeFactory.CreateScope()` без `using`. Ресурсы не освобождаются. В `ActivityLogConsumer` это сделано правильно.

- `PracticalWork.Reports.Worker/Consumers/ReportGenerateConsumer.cs`

**6. Дублирование кода между репозиториями**

`GetBookCategoryData` и маппинг сущности в `Book` один в один в `BookRepository` и `LibraryRepository`. Нарушение DRY.

**7. Доменная модель улетает в HTTP-ответ**

`ReportService.GetAllActivityLogs` возвращает `IEnumerable<ActivityLog>`, контроллер отдаёт его как `Ok(result)`. Доменные модели не должны сериализоваться в ответ API. Нужен DTO.

---

## Средние

**8. Контроллеры отдают plain text вместо JSON**

`BooksController.CreateBook`, `LibraryController.BorrowBook`, `ReaderController.CreateReader` делают `Content(result.ToString())` — возвращают Guid как текст, хотя `ProducesResponseType` обещает JSON.

**9. POST для чтения**

`LibraryController.GetBookDetailsById` и `GetBookDetailsByTitle` помечены `[HttpPost]`. Это операции чтения — должен быть `[HttpGet]`.

**10. DateTime.Now в домене**

`Borrow.ReturnBook()`, `Reader.DeActiveReader()`, `LibraryService.BorrowBook()` используют `DateTime.Now`. Жёсткая привязка к системным часам — невозможно подставить время в тестах. Нужно `TimeProvider`.

**11. BookService делает слишком много**

Зависит от 4 сервисов: репозиторий, Redis, MinIO, RabbitMQ. Управляет CRUD, кэшем, загрузкой файлов и событиями. Нарушение SRP.

**12. ReportJobService - god object**

7 зависимостей, генерирует отчёт, строит CSV, грузит в MinIO, рендерит шаблон и шлёт email. Всё в одном классе.

**13. NotificationService тоже берёт на себя много**

7 зависимостей, вытягивает данные, рендерит шаблон, отправляет email, пишет лог уведомлений.

**14. ArchiveService зависит от репозитория и сервиса одновременно**

Тянет и `IBookRepository`, и `IBookService`. Метод `GetAvailableOldBooks` надо вынести в `IBookService`.

**15. Мagic strings**

`"ReturnReminder"`, `"reports:list"`, `"reports"` — дублируются по разным файлам. Вынести в константы.

**16. Валидация в сервисе, а не в FluentValidation**

`BookService.CreateBookDetails` проверяет расширение и размер картинки. В проекте уже есть FluentValidation — проверку надо туда.

**17. RazorLightEngine через new**

`EmailTemplateService` создаёт `new RazorLightEngineBuilder()...Build()` в конструкторе. Невозможно замокать в тестах. Нужно регистрировать через DI.

---

## Низкие

**18.** Часть сервисов без `sealed` (ReportService, ArchiveService, NotificationService, ReportJobService, ReportGenService)

**19.** `FirstOrDefaultAsync` вместо `AnyAsync` в `ReportRepository` для проверки существования

**20.** Hangfire `AllowAllDashboardAuthorizationFilter` всегда возвращает `true` — доступ без авторизации

**21.** `BookNotFoundException` → HTTP 400 вместо 404 в `DomainExceptionFilter`

**22.** `RedisService` — Singleton с non-thread-safe `List<string>`, нужна `ConcurrentBag`

**23.** Worker-сервисы без глобального обработчика исключений

**24.** В проекте `Data.PostgreSql` не включён `<Nullable>enable</Nullable>`
