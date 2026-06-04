using Library.SharedKernel.Enums;

namespace Library.Models;

/// <summary>
/// Книга
/// </summary>
public sealed class Book
{
    /// <summary>Название книги</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Авторы</summary>
    public IReadOnlyList<string> Authors { get; set; } = [];

    /// <summary>Краткое описание книги</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Год издания</summary>
    public int Year { get; set; }

    /// <summary>Категория</summary>
    public BookCategory Category { get; set; }

    /// <summary>Статус</summary>
    public BookStatus Status { get; set; }

    /// <summary>Путь к изображению обложки</summary>
    public string CoverImagePath { get; set; } = string.Empty;

    /// <summary>В архиве</summary>
    public bool IsArchived { get; set; }

    /// <summary>Проверка перевода в архив</summary>
    public bool CanBeArchived() => Status != BookStatus.Borrow;

    /// <summary>Проверка выдачи на руки</summary>
    public bool CanBeBorrowed() => !IsArchived && Status == BookStatus.Available;

    /// <summary>Перевод в архив</summary>
    public void Archive()
    {
        if (!CanBeArchived())
            throw new InvalidOperationException("Книга не может быть заархивирована.");

        IsArchived = true;
        Status = BookStatus.Archived;
    }

    /// <summary>
    /// Обновление деталей
    /// </summary>
    /// <param name="description"> Краткое описание книги </param>
    /// <param name="coverImagePath"> Путь к изображению обложки </param>
    public void UpdateDetails(string description, string coverImagePath)
    {
        Description = description;
        CoverImagePath = coverImagePath;
    }
}