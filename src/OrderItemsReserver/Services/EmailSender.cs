using System.Net.Http;
using System.Text;
using System.Text.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace OrderItemsReserver.Services
{
    public class EmailSender
    {
        private string SendGridApiKey { get; }
        private string LogicAppUrl { get; }

        public EmailSender(string sendGridApiKey, string logicAppUrl)
        {
            SendGridApiKey = sendGridApiKey;
            LogicAppUrl = logicAppUrl;
        }

        public Task SendEmailAsync(string order)
        {
            var client = new HttpClient();

            var jsonData = JsonSerializer.Serialize(new
            {
                email = "dado1481@gmail.com",
                subject = "Unsuccessful Order Creation",
                content = $"The following order can not be created in the blob: {order}"
            });

            return client.PostAsync(
                this.LogicAppUrl,
                new StringContent(jsonData, Encoding.UTF8, "application/json"));
        }

        public Task SendEmailSendGridAsync(string order)
        {
            var client = new SendGridClient(this.SendGridApiKey);

            var messageText = $"The following order can not be created in the blob: {order}";

            var msg = new SendGridMessage
            {
                From = new EmailAddress("darya_paluichyk@epam.com", "DPTest"),
                Subject = "Unsuccessful Order Creation",
                PlainTextContent = messageText,
                HtmlContent = messageText
            };

            msg.AddTo(new EmailAddress("dado1481@gmail.com"));

            msg.TrackingSettings = new TrackingSettings
            {
                ClickTracking = new ClickTracking { Enable = false }
            };

            return client.SendEmailAsync(msg);
        }
    }
}
