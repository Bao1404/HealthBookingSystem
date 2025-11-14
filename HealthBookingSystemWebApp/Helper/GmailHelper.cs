using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using HealthBookingSystem.Models;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HealthBookingSystem.Helper
{
    public class GmailHelper
    {
        private readonly GmailApiOption _gmailApiOption;

        public GmailHelper(IOptions<GmailApiOption> gmailApiOption)
        {
            _gmailApiOption = gmailApiOption.Value;
        }

        static string[] Scopes = { GmailService.Scope.GmailSend };

        private async Task<UserCredential> GetCredentialsAsync()
        {
            UserCredential credential;

            using (var stream = new FileStream(_gmailApiOption.client_secret, FileMode.Open, FileAccess.Read))
            {
                var credPath = "token.json";
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
            }
            return credential;
        }

        public async Task<string> SendEmailAsync()
        {
            var credential = await GetCredentialsAsync();

            var service = new GmailService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Gmail API .NET"
            });

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MimeKit.MailboxAddress("tuanntde180464@fpt.edu.vn", "tuanntde180464@fpt.edu.vn"));
            emailMessage.To.Add(new MimeKit.MailboxAddress("nguyennttuan8604@gmail.com", "nguyennttuan8604@gmail.com"));
            emailMessage.Subject = "123123123";

            var bodyBuilder = new MimeKit.BodyBuilder
            {
                TextBody = "test"
            };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            var msg = new Google.Apis.Gmail.v1.Data.Message
            {
                Raw = Base64UrlEncode(emailMessage.ToString())
            };

            try
            {
                var request = service.Users.Messages.Send(msg, "me");
                await request.ExecuteAsync();
                return "Email sent successfully"; // Trả về thông báo thành công
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}"; // Trả về thông báo lỗi
            }
        }

        private string Base64UrlEncode(string input)
        {
            var byteArray = System.Text.Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(byteArray)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}
