namespace Library.Dto.Output;

/// <summary>
/// Информация об архивации книги
/// </summary>
public sealed class ArchiveBookDto
{
    /// <summary>Идентификатор книги</summary>
    public Guid Id { get; }
    
    /// <summary>Название книги</summary>
    public string Title { get; }

    /// <summary>Дата перевода в архив</summary>
    public DateTime ArchivedAt { get; }

    public ArchiveBookDto(Guid id, string title)
    {
        Id = id;
        Title = title;
        ArchivedAt = DateTime.UtcNow;
    }
}