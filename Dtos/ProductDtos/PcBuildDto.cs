using System;
using System.Collections.Generic;

namespace PCShop_Backend.Dtos
{
    public class PcBuildDto
    {
        public int BuildId { get; set; }
        public string BuildName { get; set; }
        public string? Description { get; set; }
        public bool IsPublic { get; set; }

        // Creator info
        public int? CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Components with full details
        public List<PcBuildComponentDto> Components { get; set; }

        // Calculated fields
        public decimal TotalPrice { get; set; }
    }
}
