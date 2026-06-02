namespace Library.BackgroundServices.Email.Abstractions.Jobs;

/// <summary>
/// Базовый интерфейс для всех фоновых задач библиотеки
/// </summary>
public interface ILibraryJob
{
    /// <summary>Уникальное имя задачи</summary>
    string JobName { get; }
    
    /// <summary>Описание задачи</summary>
    string Description { get; }
    
    /// <summary>
    /// Выполнить фоновую задачу
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken);
}