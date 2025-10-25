namespace PCShop_Backend.Dtos.OrderDtos.CreateDtos
{
    public class CreateReceiptDto
    {
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public string? PaymentMethod { get; set; }
        public string? ShippingAddress { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Notes { get; set; }
    }
}
