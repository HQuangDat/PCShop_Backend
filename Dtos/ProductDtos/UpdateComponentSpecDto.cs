using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Dtos
{
    public class UpdateComponentSpecDto
    {
        public int ComponentId { get; set; }

        [StringLength(50)]
        public string SpecKey { get; set; } = null!;

        [StringLength(255)]
        public string SpecValue { get; set; } = null!;

        public int? DisplayOrder { get; set; }
    }
}
