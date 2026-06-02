using Library.Abstraction.Services;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Library.Email;

public static class Entry
{
    public static IServiceCollection AddEmail(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<EmailOptions>().BindConfiguration("EmailSettings");
        
        serviceCollection.AddScoped<ISmtpClient>(s =>
        {
            var options = s.GetService<IOptions<EmailOptions>>()?.Value
                          ?? throw new NullReferenceException("Email settings not found");
            var client = new SmtpClient();
            client.Connect(options.SmtpServer, options.SmtpPort);
            client.AuthenticationMechanisms.Clear();
            return client;
        });
        serviceCollection.AddScoped<IEmailService, EmailService>();
        
        return serviceCollection;
    }
}