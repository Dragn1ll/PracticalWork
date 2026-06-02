namespace Library.BackgroundServices.Email.Settings;

/// <summary>
/// Настройки шаблонов email сообщений
/// </summary>
public class EmailTemplateOptions
{
    public ReturnReminderTemplate ReturnReminder { get; init; } = new();
    public WeeklyReportTemplate WeeklyReport { get; init; } = new();
    public string LibraryName { get; init; } = "Библиотека";
    public string LibraryAddress { get; init; } = "";
    public string LibraryPhone { get; init; } = "";
    public string WorkingHours { get; init; } = "";
    public string DateFormat { get; init; } = "dd.MM.yyyy";
    public string DateTimeFormat { get; init; } = "dd.MM.yyyy HH:mm";
}

public class ReturnReminderTemplate
{
    public string SubjectTemplate { get; init; } = "Напоминание о возврате книги: \"{BookTitle}\"";
    public int DaysBeforeDueDate { get; init; } = 3;
    public int IntervalInHours { get; init; } = 24;
}

public class WeeklyReportTemplate
{
    public string SubjectTemplate { get; init; } = "Еженедельный отчет библиотеки за период {StartDate} - {EndDate}";
    public string[] AdminEmails { get; init; } = [];
    public int ReportRetentionDays { get; init; } = 90;
    public int IntervalInMinutes { get; init; } = 24 * 60;
}