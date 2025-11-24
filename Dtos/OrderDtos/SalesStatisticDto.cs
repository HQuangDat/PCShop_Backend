namespace PCShop_Backend.Dtos.OrderDtos
{
    public class SalesStatisticDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalQuantitySold { get; set; } = 0;
        public decimal TotalRevenue { get; set; } = 0;
        public DateTime? Date { get; set; }
    }
}
