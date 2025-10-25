using System.ComponentModel.DataAnnotations.Schema;

namespace PCShop_Backend.Dtos.OrderDtos
{
    public class CartItemsDtos
    {
        public int CartItemId { get; set; }

        public int UserId { get; set; }

        public int? ComponentId { get; set; }

        public int? BuildId { get; set; }

        public int Quantity { get; set; } = 0;
        public DateTime? AddedAt { get; set; }
    }
}
