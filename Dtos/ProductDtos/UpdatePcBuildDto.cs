using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Dtos
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
