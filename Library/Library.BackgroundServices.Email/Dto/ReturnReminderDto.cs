namespace Library.BackgroundServices.Email.Dto;

/// <summary>
/// Модель данных для шаблона напоминания о возврате книги
/// </summary>
public class ReturnReminderDto
{
    public string ReaderName { get; set; } = null!;
    public string BookTitle { get; set; } = null!;
    public string BookAuthors { get; set; } = null!;
    public string DueDate { get; set; } = null!;
    public int DaysRemaining { get; set; }
    public string LibraryName { get; set; } = null!;
    public string LibraryAddress { get; set; } = null!;
    public string LibraryPhone { get; set; } = null!;
    public string WorkingHours { get; set; } = null!;
}