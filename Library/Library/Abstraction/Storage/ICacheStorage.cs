namespace Library.Abstraction.Storage;

/// <summary>
/// Сервис по работе с кэшем
/// </summary>
public interface ICacheStorage
{
    /// <summary>
    /// Получить значение из кэша
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <typeparam name="T">Тип возвращаемого значения</typeparam>
    /// <returns>Значение хранящееся в кэше</returns>
    Task<T?> TryGetAsync<T>(string key);

    /// <summary>
    /// Изменить/добавить значение в кэш
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    /// <param name="expiration">Время жизни кэша</param>
    /// <typeparam name="T">Тип отправляемого значения</typeparam>
    Task SetAsync<T>(string key, T value, TimeSpan expiration);

    /// <summary>
    /// Удалить значение по ключу
    /// </summary>
    /// <param name="key">Ключ</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Удаление значений по префиксу ключа
    /// </summary>
    /// <param name="prefix">Префикс ключа</param>
    Task RemoveByPrefixAsync(string prefix);
}