using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Dtos.ProductDtos.UpdateDto
{
    public class UpdateComponentSpecDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Component ID must be greater than 0.")]
        public int ComponentId { get; set; }

        [Required(ErrorMessage = "Spec key is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Spec key must be between 1 and 50 characters.")]
        public string SpecKey { get; set; } = null!;

        [Required(ErrorMessage = "Spec value is required.")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Spec value must be between 1 and 255 characters.")]
        public string SpecValue { get; set; } = null!;

        [Range(0, int.MaxValue, ErrorMessage = "Display order must be a non-negative number.")]
        public int? DisplayOrder { get; set; }
    }
}
