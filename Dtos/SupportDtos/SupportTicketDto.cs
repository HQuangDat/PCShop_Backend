using PCShop_Backend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCShop_Backend.Dtos.SupportDtos
{
    public class SupportTicketDto
    {
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
        public string? Priority { get; set; }
        public int? AssignedToUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<SupportTicketCommentDto>? Comments { get; set; } = new List<SupportTicketCommentDto>();
    }
}
