using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Dtos
{
    public class CreatePcBuildDto
    {
        [Required]
        [StringLength(100)]
        public string BuildName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsPublic { get; set; } = false;

        [Required]
        [MinLength(1, ErrorMessage = "Build must have at least one component")]
        public List<CreatePcBuildComponentDto> Components { get; set; }
    }
}
