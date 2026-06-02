using Library.BackgroundServices.Email.Dto;

namespace Library.BackgroundServices.Email.Abstractions.Services;

/// <summary>
/// Сервис для автоматической архивации старых книг
/// </summary>
public interface IArchiveService
{
    /// <summary>
    /// Архивирование старых книг
    /// </summary>
    Task<ArchiveResultDto> ArchiveOldBooks(int yearsWithoutBorrow, int maxBooksPerRun);
}