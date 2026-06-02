namespace Library.BackgroundServices.Email.Settings;

/// <summary>
/// Настройки автоматической архивации старых книг
/// </summary>
public class ArchiveOptions
{
    public int YearsWithoutBorrow { get; set; } = 3;
    public int MaxBooksPerRun { get; set; } = 100;
}