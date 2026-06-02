namespace Library.Models;

/// <summary>
/// Лог уведомлений об отправке email
/// </summary>
public class NotificationLog
{
    /// <summary>Идентификатор связанной выдачи книги</summary>
    public Guid? BorrowId { get; set; }
    
    /// <summary>Тип уведомления</summary>
    public string NotificationType { get; set; } = null!;
    
    /// <summary>Email получателя</summary>
    public string RecipientEmail { get; set; } = null!;
    
    /// <summary>Тема письма</summary>
    public string Subject { get; set; } = null!;
    
    /// <summary>Отправлено ли уведомление</summary>
    public bool IsSent { get; set; }
    
    /// <summary>Сообщение об ошибке</summary>
    public string ErrorMessage { get; set; } = string.Empty;
    
    /// <summary>Дата отправки</summary>
    public DateTime SentAt { get; set; }
}