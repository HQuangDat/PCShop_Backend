namespace PCShop_Backend.Dtos.ProductDtos.CreateDto
{
    public class CreatePcBuildComponentDto
    {
        public int ComponentId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
