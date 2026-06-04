using Library.Dto.Input;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;

namespace Library.Email.UnitTests;

public class EmailServiceTests
{
    private readonly Mock<ISmtpClient> _smtpClient = new();
 
    private EmailService Build(string server = "localhost", int port = 25, bool ssl = false) =>
        new(_smtpClient.Object, Options.Create(new EmailOptions
        {
            SmtpServer = server, SmtpPort = port, UseSsl = ssl,
            SenderName = "Библиотека", SenderEmail = "noreply@lib.local"
        }));
 
    [Fact]
    public async Task SendAsync_WhenSmtpSucceeds_ReturnsIsSuccessTrue()
    {
        _smtpClient.Setup(c => c.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(),
            It.IsAny<SecureSocketOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _smtpClient.Setup(c => c.SendAsync(It.IsAny<MimeMessage>(),
            It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .ReturnsAsync("OK");
 
        var result = await Build().SendAsync(new EmailMessageDto
        {
            EmailTo = "reader@test.com", Subject = "Тест", Body = "Текст", IsHtml = false
        });
 
        Assert.True(result.IsSuccess);
        Assert.Equal("OK", result.Message);
    }
 
    [Fact]
    public async Task SendAsync_WhenSmtpThrows_ReturnsIsSuccessFalseWithMessage()
    {
        _smtpClient.Setup(c => c.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(),
            It.IsAny<SecureSocketOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Connection refused"));
 
        var result = await Build().SendAsync(new EmailMessageDto
        {
            EmailTo = "reader@test.com", Subject = "Тест", Body = "Текст"
        });
 
        Assert.False(result.IsSuccess);
        Assert.Contains("Connection refused", result.Message);
    }
 
    [Fact]
    public async Task SendAsync_NeverThrowsException_AlwaysReturnsResult()
    {
        _smtpClient.Setup(c => c.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(),
            It.IsAny<SecureSocketOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Неожиданная ошибка"));
 
        var ex = await Record.ExceptionAsync(() => Build().SendAsync(new EmailMessageDto
        {
            EmailTo = "x@y.com", Subject = "S", Body = "B"
        }));
 
        Assert.Null(ex);
    }
 
    [Fact]
    public async Task SendAsync_BuildsMimeMessageWithCorrectSenderAndRecipient()
    {
        MimeMessage? captured = null;
        _smtpClient.Setup(c => c.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(),
            It.IsAny<SecureSocketOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _smtpClient.Setup(c => c.SendAsync(It.IsAny<MimeMessage>(),
            It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .Callback<MimeMessage, CancellationToken, ITransferProgress>((m, _, _) => captured = m)
            .ReturnsAsync("OK");
 
        await Build().SendAsync(new EmailMessageDto
        {
            EmailTo = "reader@test.com", RecipientName = "Иван",
            Subject = "Привет", Body = "<p>Тело</p>", IsHtml = true
        });
 
        Assert.NotNull(captured);
        var from = captured.From.Mailboxes.First();
        Assert.Equal("noreply@lib.local", from.Address);
        Assert.Equal("Библиотека", from.Name);
        var to = captured.To.Mailboxes.First();
        Assert.Equal("reader@test.com", to.Address);
    }
 
    [Fact]
    public async Task SendAsync_WhenIsHtmlTrue_BuildsHtmlBody()
    {
        MimeMessage? captured = null;
        _smtpClient.Setup(c => c.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(),
            It.IsAny<SecureSocketOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _smtpClient.Setup(c => c.SendAsync(It.IsAny<MimeMessage>(),
            It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .Callback<MimeMessage, CancellationToken, ITransferProgress>((m, _, _) => captured = m)
            .ReturnsAsync("OK");
 
        await Build().SendAsync(new EmailMessageDto
        {
            EmailTo = "r@t.com", Subject = "S", Body = "<b>Жирный</b>", IsHtml = true
        });
 
        Assert.NotNull(captured!.HtmlBody);
        Assert.Contains("Жирный", captured.HtmlBody);
    }
 
    [Fact]
    public async Task SendAsync_WhenIsHtmlFalse_BuildsTextBody()
    {
        MimeMessage? captured = null;
        _smtpClient.Setup(c => c.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(),
            It.IsAny<SecureSocketOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _smtpClient.Setup(c => c.SendAsync(It.IsAny<MimeMessage>(),
            It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .Callback<MimeMessage, CancellationToken, ITransferProgress>((m, _, _) => captured = m)
            .ReturnsAsync("OK");
 
        await Build().SendAsync(new EmailMessageDto
        {
            EmailTo = "r@t.com", Subject = "S", Body = "Простой текст", IsHtml = false
        });
 
        Assert.NotNull(captured!.TextBody);
        Assert.Contains("Простой текст", captured.TextBody);
    }
}