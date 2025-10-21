using PCShop_Backend.Dtos.ProductDtos.CreateDto;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Dtos.ProductDtos.UpdateDto
{
    public class UpdatePcBuildDto
    {
        [Required]
        [StringLength(100)]
        public string BuildName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsPublic { get; set; }

        // Allow updating components
        public List<CreatePcBuildComponentDto>? Components { get; set; }
    }
}
