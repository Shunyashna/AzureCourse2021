using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace OrderItemsReserver.Services
{
    public class EmailSender
    {
        private string ApiKey { get; }

        public EmailSender(string apiKey)
        {
            ApiKey = apiKey;
        }

        public Task SendEmailAsync(string order)
        {
            var client = new SendGridClient(this.ApiKey);

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
