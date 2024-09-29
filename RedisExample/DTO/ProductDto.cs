namespace RedisExample.DTO
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        // Foreign key for Category
        public int CategoryId { get; set; }

    }
}
