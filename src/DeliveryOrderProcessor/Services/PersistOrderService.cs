using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using DeliveryOrderProcessor.Models;

namespace DeliveryOrderProcessor.Services
{
    public class PersistOrderService
    {
        private static readonly string connectionString = "AccountEndpoint=https://azurecourse2021cosmosdb.documents.azure.com:443/;AccountKey=dC0SJvqmTfl3Un4CirKeyjc89hJh1nPwP47lOgpZrrNInxlXG02qPu9TfyziqyiIJLDrZpT2sJqFak1rcEH3Zw==;";

        public async void CreateOrder(OrderDetailsRequest orderDetails)
        {
            orderDetails.Id = Guid.NewGuid().ToString();

            var options = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions()
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };

            using (CosmosClient client = new CosmosClient(connectionString, options))
            {
                /*DatabaseResponse databaseResponse = await client.CreateDatabaseIfNotExistsAsync("Products");
                Database targetDatabase = databaseResponse.Database;*/
                var container = client.GetContainer("Orders", "OrdersContainer");
                var task = container.CreateItemAsync(orderDetails, new PartitionKey(orderDetails.Id));
                Task.WaitAll(task);
            }
        }
    }
}
