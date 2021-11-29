using System;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderItemsReserver.Models;
using OrderItemsReserver.Services;
using Microsoft.Azure.ServiceBus;

namespace OrderItemsReserver
{
    public static class CreateOrderFunction
    {
        const string QueueName = "ordersmessages";

        [FunctionName("CreateOrder")]
        public static async Task Run(
            [ServiceBusTrigger(QueueName, Connection = "ServiceBusConnectionString")]
            string myQueueItem,
            ILogger log)
        {
            var orderDetails = JsonConvert.DeserializeObject<OrderDetailsRequest>(myQueueItem);

            if (orderDetails?.Orders == null || !orderDetails.Orders.Any())
            {
                return;
            }

            var queueClient =
                new QueueClient(Environment.GetEnvironmentVariable("ServiceBusConnectionString"), QueueName);

            var service = new OrderBlobService();

            var retryCounter = 3;
            Response<BlobContentInfo> result;

            while (retryCounter > 0)
            {
                try
                {
                    result = service.CreateOrderFile(myQueueItem, log);

                    if (result?.Value != null)
                    {
                        log.Log(LogLevel.Critical, $"Break: counter {retryCounter}");
                        break;
                    }
                }
                catch
                {
                    log.Log(LogLevel.Critical, $"Retry count: { 4 - retryCounter}");
                }

                retryCounter--;
            }

            if (retryCounter == 0)
            {
                // Send to email
                log.Log(LogLevel.Critical, $"Sending the email.");
                var sender = new EmailSender(Environment.GetEnvironmentVariable("SendGridApiKey"),
                    Environment.GetEnvironmentVariable("LogicAppUrl"));
                await sender.SendEmailAsync(myQueueItem);
            }

            await queueClient.CloseAsync();
        }
    }
}
