namespace Library.Dto.Output;

/// <summary>
/// Результат отправки email
/// </summary>
public sealed class EmailSendResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}