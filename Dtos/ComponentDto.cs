namespace PCShop_Backend.Dtos
{
    public class ComponentDto
    {
        public int ComponentId { get; set; }
        public string? Name { get; set; }
        public string? CategoryName { get; set; }
        public string? Brand { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<ComponentSpecDto> Specs { get; set; }
    }
}
