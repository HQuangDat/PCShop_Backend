using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCShop_Backend.Dtos.OrderDtos
{
    public class ReceiptItemsDto
    {
        public int ReceiptItemId { get; set; }
        public int ReceiptId { get; set; }
        public int? ComponentId { get; set; }
        public int? BuildId { get; set; }
        public string ItemName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
