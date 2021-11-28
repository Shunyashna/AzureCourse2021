using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderItemsReserver.Models;
using OrderItemsReserver.Services;
using Microsoft.Azure.ServiceBus;
using Polly;

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
            Response<BlobContentInfo> result = null;

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
                var sender = new EmailSender("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");
                await sender.SendEmailAsync(myQueueItem);
            }

            /*// Execute a function returning a result
            var result = Policy
                .Handle<InvalidDataException>()
                .Or<Exception>()
                .Retry(3, (exception, retryCount, context) =>
                {
                    log.Log(LogLevel.Critical, exception, $"Retry count: {retryCount}");
                })
                .Execute(() => service.CreateOrderFile(myQueueItem, log));*/

            await queueClient.CloseAsync();
        }

        /*[FunctionName("CreateOrder")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var orderDetails = JsonConvert.DeserializeObject<OrderDetailsRequest>(requestBody);

            if (orderDetails?.Orders == null || !orderDetails.Orders.Any())
            {
                return new BadRequestResult();
            }

            var service = new OrderBlobService();
            await service.CreateOrderFile(requestBody);
            
            return new OkResult();
        }*/
    }
}
