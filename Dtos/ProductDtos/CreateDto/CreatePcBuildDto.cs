namespace PCShop_Backend.Dtos.ProductDtos.CreateDto
{
    public class CreatePcBuildDto
    {
        public required string BuildName { get; set; }
        public string? Description { get; set; }
        public bool IsPublic { get; set; } = false;
        public required List<CreatePcBuildComponentDto> Components { get; set; }
    }
}
