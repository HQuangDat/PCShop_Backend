using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Dtos
{
    public class CreatePcBuildComponentDto
    {
        [Required]
        public int ComponentId { get; set; }

        [Range(1, 10)]
        public int Quantity { get; set; } = 1;
    }
}