using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PCShop_Backend.Models;

[Index("AssignedToUserId", Name = "IX_Tickets_AssignedToUserId")]
[Index("Status", Name = "IX_Tickets_Status")]
[Index("UserId", Name = "IX_Tickets_UserId")]
public partial class Ticket
{
    [Key]
    public int TicketId { get; set; }

    public int UserId { get; set; }

    [StringLength(100)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [StringLength(20)]
    public string? Priority { get; set; }

    public int? AssignedToUserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("AssignedToUserId")]
    [InverseProperty("TicketAssignedToUsers")]
    public virtual User? AssignedToUser { get; set; }

    [InverseProperty("Ticket")]
    public virtual ICollection<TicketComment> TicketComments { get; set; } = new List<TicketComment>();

    [ForeignKey("UserId")]
    [InverseProperty("TicketUsers")]
    public virtual User User { get; set; } = null!;
}
