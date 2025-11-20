namespace PCShop_Backend.Dtos.CartDtos.CreateDtos
{
    public class AddItemToCartDtos
    {
        public int? ComponentId { get; set; }
        public int? BuildId { get; set; }
        public int Quantity { get; set; }
    }
}
