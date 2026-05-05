using PCShop_Backend.Dtos.ProductDtos.CreateDto;

namespace PCShop_Backend.Dtos.ProductDtos.UpdateDto
{
    public class UpdatePcBuildDto
    {
        public required string BuildName { get; set; }
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public List<CreatePcBuildComponentDto>? Components { get; set; }
    }
}
