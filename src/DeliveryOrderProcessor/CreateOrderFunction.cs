using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderPersister.Models;
using OrderPersister.Services;

namespace OrderPersister
{
    public static class CreateOrderFunction
    {
        [FunctionName("CreateOrder")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var orderDetails = JsonConvert.DeserializeObject<OrderDetailsRequest>(requestBody);

            if (orderDetails?.Items == null || !orderDetails.Items.Any())
            {
                return new BadRequestResult();
            }

            var service = new PersistOrderService();
            service.CreateOrder(orderDetails);

            return new OkResult();
        }
    }
}
