namespace PCShop_Backend.Dtos.ProductDtos.UpdateDto
{
    public class updateComponentDto
    {
        public string Name { get; set; } = "";
        public int CategoryId { get; set; }
        public string? Brand { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }
        public string? Description { get; set; }
    }
}
