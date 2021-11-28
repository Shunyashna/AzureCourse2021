namespace Microsoft.eShopWeb.ApplicationCore.Models
{
    public class EndpointsConfiguration
    {
        public const string ConfigName = "endpoints";

        public string BlobFunction { get; set; }
        public string CosmosDbFunction { get; set; }
        public string ServiceBus { get; set; }
    }
}
