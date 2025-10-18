using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Dtos
{
    public class createComponentDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        [Required]
        public int CategoryId { get; set; }

        public string? Brand { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public bool? IsActive { get; set; }

        public string? Description { get; set; }
    }
}
