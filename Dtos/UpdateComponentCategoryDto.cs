using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Dtos
{
    public class UpdateComponentCategoryDto
    {
        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; } = null!;

        [StringLength(255)]
        public string? Description { get; set; }
    }
}
