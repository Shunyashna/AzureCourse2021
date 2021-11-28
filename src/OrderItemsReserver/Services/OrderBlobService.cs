using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace OrderItemsReserver.Services
{
    /// <summary>
    /// Defines the blob storage service.
    /// </summary>
    public class OrderBlobService
    {
        public Response<BlobContentInfo> CreateOrderFile(string data, ILogger logger)
        {
            var connectionString =
                "1DefaultEndpointsProtocol=https;AccountName=course2021storageaccount;AccountKey=AI7aRS/QKssHU/tQOjhafNjuLUcS2fYrBDR4DZEBizY0iSTD9b37aqYc9KncV97FTrM3eCXJlqCgN064tdxqCA==;EndpointSuffix=core.windows.net";
            var containerName = "orders";

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Get the container (folder) the file will be saved in
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            containerClient.CreateIfNotExistsAsync();

            // Get the Blob Client used to interact with (including create) the blob
            BlobClient blobClient = containerClient.GetBlobClient($"{DateTime.Now.ToUniversalTime()}.json");
            var task = blobClient.UploadAsync(new BinaryData(data));
            var result = task.GetAwaiter().GetResult();

            logger.Log(LogLevel.Critical, $"Blob result: {result}");

            if (result?.Value == null)
            {
                throw new InvalidDataException("Could not create order blob.");
            }

            return result;
        }
    }
}
