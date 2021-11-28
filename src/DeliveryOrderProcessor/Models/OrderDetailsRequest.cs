using System;
using System.Collections.Generic;

namespace OrderPersister.Models
{
    public class OrderDetailsRequest
    {
        public string Id { get; set; }

        public Address ShippingAddress { get; set; }

        public List<Item> Items { get; set; } = new List<Item>();

        public Decimal FinalPrice
        {
            get {
                var total = 0m;
                foreach (var item in this.Items)
                {
                    total += item.UnitPrice * item.Quantity;
                }
                return total;
            }
        }
    }
}
