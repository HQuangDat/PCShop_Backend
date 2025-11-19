using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Dtos.ProductDtos.CreateDto
{
    public class CreateComponentCategoryDto
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Category name must be between 1 and 50 characters.")]
        public string CategoryName { get; set; } = "";

        [StringLength(255)]
        public string? Description { get; set; }
    }
}
