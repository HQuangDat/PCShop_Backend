using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Dtos.ProductDtos.CreateDto
{
    public class CreateComponentCategoryDto
    {
        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; } = "";

        [StringLength(255)]
        public string? Description { get; set; }
    }
}
