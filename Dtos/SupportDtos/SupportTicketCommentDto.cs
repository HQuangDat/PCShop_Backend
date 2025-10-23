using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCShop_Backend.Dtos.SupportDtos
{
    public class SupportTicketCommentDto
    {
        public int CommentId { get; set; }
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public string CommentText { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }
}
