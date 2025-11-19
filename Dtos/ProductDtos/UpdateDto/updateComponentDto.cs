using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Dtos.ProductDtos.UpdateDto
{
    public class updateComponentDto
    {
        [Required(ErrorMessage = "Component name is required.")]
        [MaxLength(100, ErrorMessage = "Component name must not exceed 100 characters.")]
        public string Name { get; set; } = "";

        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be greater than 0.")]
        public int CategoryId { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be a non-negative number.")]
        public int StockQuantity { get; set; }

        [StringLength(255)]
        [Url(ErrorMessage = "Invalid image URL format.")]
        public string? ImageUrl { get; set; }

        public bool? IsActive { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }
    }
}
