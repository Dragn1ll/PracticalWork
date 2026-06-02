namespace Library.Contracts.v1.Reader.Request;

/// <summary>
/// Запрос на продление карточки читателя
/// </summary>
/// <param name="NewExpiryDate">Новая дата окончания действия карточки читателя</param>
public record ExtendReaderRequest(DateOnly NewExpiryDate);