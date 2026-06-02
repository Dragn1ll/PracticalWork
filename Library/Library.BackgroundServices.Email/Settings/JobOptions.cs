namespace Library.BackgroundServices.Email.Settings;

/// <summary>
/// Настройки фоновых задач системы библиотеки
/// </summary>
public class JobOptions
{
    public Dictionary<string, JobConfiguration> Jobs { get; set; } = new();
}

/// <summary>
/// Конфигурация отдельной фоновой задачи
/// </summary>
public class JobConfiguration
{
    /// <summary>Cron выражение для планирования</summary>
    public string CronExpression { get; set; } = "";
    public int MaxRetries { get; set; } = 3;
    public int TimeoutMinutes { get; set; } = 30;
}