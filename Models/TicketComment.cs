using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PCShop_Backend.Models;

public partial class TicketComment
{
    [Key]
    public int CommentId { get; set; }

    public int TicketId { get; set; }

    public int UserId { get; set; }

    public string CommentText { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("TicketId")]
    [InverseProperty("TicketComments")]
    public virtual Ticket Ticket { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("TicketComments")]
    public virtual User User { get; set; } = null!;
}
