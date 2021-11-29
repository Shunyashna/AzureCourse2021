using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using DeliveryOrderProcessor.Models;

namespace DeliveryOrderProcessor.Services
{
    public class PersistOrderService
    {
        private static readonly string connectionString = "AccountEndpoint=https://azurecourse2021cosmosdb.documents.azure.com:443/;AccountKey=7HAQjj9shwjFhOvfg5S2saK7MhZwaHKMKZb5VbqUMBnL7oLuWDd5tLD5fEGpSVq7escBQdXhBWd5BDio2qN7iA==;";

        public async void CreateOrder(OrderDetailsRequest orderDetails)
        {
            orderDetails.Id = Guid.NewGuid().ToString();

            var options = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };

            using (CosmosClient client = new CosmosClient(connectionString, options))
            {
                var container = client.GetContainer("Orders", "OrdersContainer");
                var task = container.CreateItemAsync(orderDetails, new PartitionKey(orderDetails.Id));
                Task.WaitAll(task);
            }
        }
    }
}
