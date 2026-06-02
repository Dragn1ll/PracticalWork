namespace Library.Models;

/// <summary>
/// Читатель
/// </summary>
public sealed class Reader
{
    /// <summary>ФИО читателя</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Номер телефона</summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Дата окончания действия</summary>
    public DateOnly ExpiryDate { get; set; }

    /// <summary>Активность карточки</summary>
    public bool IsActive { get; set; }

    /// <summary>Электронная почта читателя</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Проверка активности карточки</summary>
    public bool CanDoSomething() => IsActive && ExpiryDate >= DateOnly.FromDateTime(DateTime.UtcNow);
    
    /// <summary>
    /// Обновление даты окончания действия
    /// </summary>
    /// <param name="newExpiryDate">Новая дата</param>
    public void UpdateExpiryDate(DateOnly newExpiryDate)
    {
        if (!CanDoSomething())
        {
            throw new InvalidOperationException("Карточка не может быть продлена!");
        }
        
        ExpiryDate = newExpiryDate;
    }

    /// <summary>
    /// Закрытие карточки читателя
    /// </summary>
    public void DeActiveReader()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Карточка не может быть закрыта, так как уже закрыта!");
        }
        
        IsActive = false;
        ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}