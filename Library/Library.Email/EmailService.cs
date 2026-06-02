using Library.Abstraction.Services;
using Library.Dto.Input;
using Library.Dto.Output;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Library.Email;

/// <inheritdoc cref="IEmailService"/>
public class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ISmtpClient _client;

    public EmailService(ISmtpClient client, IOptions<EmailOptions> settings)
    {
        _options = settings.Value;
        _client = client;
    }

    /// <inheritdoc cref="IEmailService.SendAsync"/>
    public async Task<EmailSendResultDto> SendAsync(EmailMessageDto message)
    {
        EmailSendResultDto result = new();
        try
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
            mimeMessage.To.Add(new MailboxAddress(message.RecipientName, message.EmailTo));
            mimeMessage.Subject = message.Subject;
            mimeMessage.Body = GetBodyBuilder(message).ToMessageBody();

            var response = await _client.SendAsync(mimeMessage);
            result.IsSuccess = true;
            result.Message = response;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Message = ex.Message;
        }

        return result;
    }
    
    private BodyBuilder GetBodyBuilder(EmailMessageDto message)
    {
        var builder = new BodyBuilder();
        if (message.IsHtml)
        {
            builder.HtmlBody = message.Body;
        }
        else
        {
            builder.TextBody = message.Body;
        }
        return builder;
    }
}