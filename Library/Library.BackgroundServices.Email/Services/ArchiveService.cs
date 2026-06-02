using System.Diagnostics;
using Library.Abstraction.Services;
using Library.Abstraction.Storage.Repositories;
using Library.BackgroundServices.Email.Abstractions.Services;
using Library.BackgroundServices.Email.Dto;
using Library.Dto.Output;
using Library.SharedKernel.Events.Book;
using Microsoft.Extensions.Logging;

namespace Library.BackgroundServices.Email.Services;

/// <inheritdoc cref="IArchiveService"/>
public class ArchiveService : IArchiveService
{
    private readonly IBookRepository _bookRepository;
    private readonly IBookService _bookService;
    private readonly ILibraryEventProducer _producer;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ArchiveService> _logger;

    public ArchiveService(
        IBookRepository bookRepository,
        IBookService bookService,
        ILibraryEventProducer producer, 
        TimeProvider timeProvider,
        ILogger<ArchiveService> logger)
    {
        _bookRepository = bookRepository;
        _bookService = bookService;
        _producer = producer;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <inheritdoc cref="IArchiveService.ArchiveOldBooks"/>
    public async Task<ArchiveResultDto> ArchiveOldBooks(int yearsWithoutBorrow, int maxBooksPerRun)
    {
        var stopWatch = Stopwatch.StartNew();
        
        var cutoffDate = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime.AddYears(-yearsWithoutBorrow));
        var books = await _bookRepository.GetAvailableOldBooks(cutoffDate, 1, maxBooksPerRun);
        var archiveResult = await ArchiveOldBooksAsync(books, yearsWithoutBorrow);

        stopWatch.Stop();
        archiveResult.ExecutionTime = stopWatch.Elapsed;

        return archiveResult;
    }

    private async Task<ArchiveResultDto> ArchiveOldBooksAsync(IList<AvailableOldBookDto> books, int yearsWithoutBorrow)
    {
        var archiveResult = new ArchiveResultDto();
        var skipReasons = new HashSet<string>();

        foreach (var book in books)
        {
            try
            {
                var archivedBook = await _bookService.ArchiveBook(book.Id);
                
                await PublishArchivedBookEventAsync(archivedBook, yearsWithoutBorrow);
                
                archiveResult.ArchivedCount++;
                _logger.LogInformation("Книга '{Title}' (ID: {Id}) заархивирована", book.Title, book.Id);
            }
            catch (Exception exception)
            {
                archiveResult.SkippedCount++;
                skipReasons.Add(exception.Message);
                _logger.LogError(exception, "Ошибка архивации книги '{Title}' (ID: {Id})", book.Title, book.Id);
            }
            
            archiveResult.TotalProcessed++;
        }
        
        archiveResult.SkipReasons = string.Join(";\n", skipReasons);
        return archiveResult;
    }

    private async Task PublishArchivedBookEventAsync(ArchiveBookDto book, int yearsWithoutBorrow)
    {
        var archivedEvent = new BookArchivedEvent(
            book.Id,
            book.Title,
            $"Автоматическая архивация: книга не выдавалась более {yearsWithoutBorrow} лет",
            DateTime.UtcNow);

        await _producer.PublishEventAsync(archivedEvent);
    }
}