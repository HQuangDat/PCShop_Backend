namespace PCShop_Backend.Dtos.ProductDtos.UpdateDto
{
    public class UpdateComponentSpecDto
    {
        public int ComponentId { get; set; }
        public string SpecKey { get; set; } = null!;
        public string SpecValue { get; set; } = null!;
        public int? DisplayOrder { get; set; }
    }
}
