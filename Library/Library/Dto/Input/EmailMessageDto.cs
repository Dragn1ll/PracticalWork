namespace Library.Dto.Input;

/// <summary>
/// Email сообщение для отправки
/// </summary>
public sealed class EmailMessageDto
{
    public string RecipientName { get; set; } = string.Empty;
    public string EmailTo { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public bool IsHtml { get; set; } = true;
    public string BodyEncoding { get; set; } = "UTF-8";
    public string SubjectEncoding { get; set; } = "UTF-8";
}