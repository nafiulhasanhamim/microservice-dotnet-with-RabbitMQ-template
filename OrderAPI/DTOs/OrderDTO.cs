namespace OrderAPI.DTOs
{
    public class OrderCreateDto
    {
        // public Guid OrderId { get; set; }
        // public DateTime OrderDate { get; set; }
        public string ProductId { get; set; } = null!;
        public int Quantity { get; set; }

        public string UserId { get; set; } = null!;
    }
    public class OrderReadDto
    {
        public Guid OrderId { get; set; }
        public string ProductId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public int Quantity { get; set; }
        public string UserValidationStatus { get; set; } = null!;
        public  string StockValidationStatus { get; set; } = null!;
        public string OrderStatus { get; set; } = null!;
        public DateTime OrderDate { get; set; }
    }
    public class OrderCreatedEventDto
    {
        public Guid OrderId { get; set; }
        public string ProductId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public int Quantity { get; set; }
    }



}