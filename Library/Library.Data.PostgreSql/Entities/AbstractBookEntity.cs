using Library.Abstraction.Storage.Entity;
using Library.SharedKernel.Enums;

namespace Library.Data.PostgreSql.Entities;

/// <summary>
/// Базовый класс для книг
/// </summary>
public abstract class AbstractBookEntity : EntityBase
{
    /// <summary>Название книги</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Авторы</summary>
    public IReadOnlyList<string> Authors { get; set; } = [];

    /// <summary>Краткое описание книги</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Год издания</summary>
    public int Year { get; set; }

    /// <summary>Статус</summary>
    public BookStatus Status { get; set; }

    /// <summary>Путь к изображению обложки</summary>
    public string CoverImagePath { get; set; } = string.Empty;

    /// <summary>Записи о выдачи книги</summary>
    public ICollection<BookBorrowEntity> IssuanceRecords { get; set; } = [];
}