using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderAPI.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string ProductId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public int Quantity { get; set; }
        public string UserValidationStatus { get; set; } = null!;
        public string StockValidationStatus { get; set; } = null!;
        public string OrderStatus { get; set; } = null!;
    }
}