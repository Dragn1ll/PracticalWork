using Library.BackgroundServices.Email.Abstractions.Jobs;
using Library.BackgroundServices.Email.Abstractions.Services;
using Library.BackgroundServices.Email.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Library.BackgroundServices.Email.Jobs;

/// <summary>
/// Фоновая задача: автоматическая архивация старых книг
/// </summary>
public class ArchiveOldBooksJob : ILibraryJob
{
    public string JobName => "ArchiveOldBooks";
    public string Description => "Автоматическая архивация старых книг";

    private readonly IArchiveService _archiveService;
    private readonly ArchiveOptions _archiveOptions;
    private readonly ILogger<ArchiveOldBooksJob> _logger;

    public ArchiveOldBooksJob(
        IArchiveService archiveService,
        IOptions<ArchiveOptions> archiveSettings,
        ILogger<ArchiveOldBooksJob> logger)
    {
        _archiveService = archiveService;
        _archiveOptions = archiveSettings.Value;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Начало выполнения задачи: {JobName}", JobName);
        
        var archiveResult = await _archiveService.ArchiveOldBooks(
            _archiveOptions.YearsWithoutBorrow,
            _archiveOptions.MaxBooksPerRun);

        _logger.LogInformation(
            "Архивация завершена: " +
            "обработано {Total}, " +
            "заархивировано {Archived}, " +
            "пропущено {Skipped}, " +
            "время выполнения {Time}",
            archiveResult.TotalProcessed, archiveResult.ArchivedCount, 
            archiveResult.SkippedCount, archiveResult.ExecutionTime);
    }
}