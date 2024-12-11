namespace ProductAPI.Models
{
    public class Product
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }


    }
}
