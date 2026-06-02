namespace Reports.SharedKernel.Events;

/// <summary>
/// Базовый рекорд для всех событий в сервисе Библиотека
/// </summary>
public abstract record BaseLibraryEvent(string EventType)
    : BaseEvent(Guid.NewGuid(), EventType, "library-service")
{
    /// <summary>
    /// Защищенный конструктор для наследников
    /// </summary>
    protected BaseLibraryEvent() : this(string.Empty)
    {
    }
}