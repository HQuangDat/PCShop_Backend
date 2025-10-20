
namespace PCShop_Backend.Dtos
{
    public class PcBuildComponentDto
    {
        public int ComponentId { get; set; }
        public string ComponentName { get; set; }
        public string CategoryName { get; set; }
        public string? Brand { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; } 
        public string? ImageUrl { get; set; }
    }
}
