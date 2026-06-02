namespace Library.Contracts.v1.Reader.Request;

/// <summary>
/// Запрос на создание карточки читателя
/// </summary>
/// <param name="FullName">ФИО читателя</param>
/// <param name="PhoneNumber">Номер телефона читателя</param>
/// <param name="ExpiryDate">Дата окончания действия карточки читателя</param>
public record CreateReaderRequest(
    string FullName, 
    string PhoneNumber, 
    DateOnly ExpiryDate
    );