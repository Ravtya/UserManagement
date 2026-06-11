using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace UserManagement.Services;

public class EmailService(IDataProtectionProvider provider)
{
    private readonly IDataProtector _protector = provider.CreateProtector("Email");

    private readonly string? _apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");

    public async Task SendConfirmationEmailAsync(string userEmail, string userName, string confirmationLink)
    {
        var client = new SendGridClient(_apiKey);

        var from = new EmailAddress("artem.ravtovitch@gmail.com", "User Management App");
        var to = new EmailAddress(userEmail, userName);
        const string subject = "Confirm your email address";

        var plainTextContent = $"""

                                    Confirm Your Email Address

                                    Hello {userName},

                                    Please confirm your email: {confirmationLink}

                                    Ignore this email if you didn't sign up.

                                    Best regards, 
                                    User Management App
                                            
                                """;

        var htmlContent = $"""

                           Hello <strong>{userName}</strong>,<br><br>
                           Confirm your email: <a href='{confirmationLink}'>Click here</a><br><br>
                           <br><br>
                           Ignore this email if you didn't sign up.<br><br>
                           Best regards,<br>
                           User Management App

                           """;

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        await client.SendEmailAsync(msg);
    }

    public string GenerateConfirmationLink(string email, string baseUrl)
    {
        var protectedEmail = _protector.Protect(email);
        var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(protectedEmail));

        return $"{baseUrl}/Account/ConfirmEmail?token={token}";
    }

    public string DecodeConfirmationLink(string token)
    {
        try
        {
            var protectedEmail = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            return _protector.Unprotect(protectedEmail);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}