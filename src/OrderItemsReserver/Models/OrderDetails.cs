namespace OrderItemsReserver.Models
{
    /// <summary>
    /// Defines the order details model. 
    /// </summary>
    public class OrderDetails
    {
        /// <summary>
        /// Gets or sets an item id.
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Gets or sets a product count.
        /// </summary>
        public int Quantity { get; set; }
    }
}
