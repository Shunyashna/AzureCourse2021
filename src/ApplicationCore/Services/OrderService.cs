using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using RestSharp;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Models;
using Microsoft.Azure.ServiceBus;
using System.Text.Json;

namespace Microsoft.eShopWeb.ApplicationCore.Services
{
    public partial class OrderService : IOrderService
    {
        private readonly IAsyncRepository<Order> _orderRepository;
        private readonly IUriComposer _uriComposer;
        private readonly IAsyncRepository<Basket> _basketRepository;
        private readonly IAsyncRepository<CatalogItem> _itemRepository;
        private readonly IQueueClient _queueClient;
        private readonly EndpointsConfiguration _endpoints;

        public OrderService(
            IAsyncRepository<Basket> basketRepository,
            IAsyncRepository<CatalogItem> itemRepository,
            IAsyncRepository<Order> orderRepository,
            IUriComposer uriComposer,
            IQueueClient queueClient,
            EndpointsConfiguration endpoints)
        {
            _orderRepository = orderRepository;
            _uriComposer = uriComposer;
            _basketRepository = basketRepository;
            _itemRepository = itemRepository;
            _endpoints = endpoints;
            _queueClient = queueClient;
        }

        public async Task CreateOrderAsync(int basketId, Address shippingAddress)
        {
            var basketSpec = new BasketWithItemsSpecification(basketId);
            var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);

            Guard.Against.NullBasket(basketId, basket);
            Guard.Against.EmptyBasketOnCheckout(basket.Items);

            var catalogItemsSpecification = new CatalogItemsSpecification(basket.Items.Select(item => item.CatalogItemId).ToArray());
            var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);

            var items = basket.Items.Select(basketItem =>
            {
                var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
                var itemOrdered = new CatalogItemOrdered(catalogItem.Id, catalogItem.Name, _uriComposer.ComposePicUri(catalogItem.PictureUri));
                var orderItem = new OrderItem(itemOrdered, basketItem.UnitPrice, basketItem.Quantity);
                return orderItem;
            }).ToList();

            var order = new Order(basket.BuyerId, shippingAddress, items);

            await _orderRepository.AddAsync(order);

            this.CreateOrderMessage(basket.Items.Select(basketItem => new OrderDetails
                {ItemId = basketItem.CatalogItemId, Quantity = basketItem.Quantity}));
            
            this.PersistOrder(order, _endpoints.CosmosDbFunction);
        }

        private void PersistOrder(Order order, string endpoint)
        {
            RestClient client = new RestClient(endpoint);

            RestRequest request = new RestRequest
            {
                Method = Method.POST,
                RequestFormat = DataFormat.Json
            };

            if (order?.OrderItems != null && order.OrderItems.Any())
            {
                request.AddJsonBody(
                    new
                    {
                        ShippingAddress = order.ShipToAddress,
                        Items = order.OrderItems.Select(item => new
                        {
                            CatalogItemId = item.ItemOrdered.CatalogItemId,
                            ProductName = item.ItemOrdered.ProductName,
                            UnitPrice = item.UnitPrice,
                            Quantity = item.Units
                        })
                    });
            }

            client.Execute(request);
        }

        private async void CreateOrderMessage(IEnumerable<OrderDetails> details)
        {
            var serializedOrderDetails = JsonSerializer.Serialize(new
            {
                Orders = details
            });
            var message = new Message(Encoding.UTF8.GetBytes(serializedOrderDetails));
            await _queueClient.SendAsync(message);
        }
    }
}
