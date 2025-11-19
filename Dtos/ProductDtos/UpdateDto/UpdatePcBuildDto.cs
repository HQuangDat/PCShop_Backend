using PCShop_Backend.Dtos.ProductDtos.CreateDto;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Dtos.ProductDtos.UpdateDto
{
    public class UpdatePcBuildDto
    {
        [Required(ErrorMessage = "Build name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Build name must be between 1 and 100 characters.")]
        public string BuildName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsPublic { get; set; }

        // Allow updating components
        [MinLength(1, ErrorMessage = "Build must have at least one component")]
        public List<CreatePcBuildComponentDto>? Components { get; set; }
    }
}
