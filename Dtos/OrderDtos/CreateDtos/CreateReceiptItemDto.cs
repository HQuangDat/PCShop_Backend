namespace PCShop_Backend.Dtos.OrderDtos.CreateDtos
{
    public class CreateReceiptItemDto
    {
        public int ReceiptId { get; set; }
        public int? ComponentId { get; set; }
        public int? BuildId { get; set; }
        public string ItemName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
