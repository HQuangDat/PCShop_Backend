using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PCShop_Backend.Models;

[Index("Email", Name = "IX_Users_Email")]
[Index("IsActive", Name = "IX_Users_IsActive")]
[Index("RoleId", Name = "IX_Users_RoleId")]
[Index("Username", Name = "UQ__Users__536C85E477ED2487", IsUnique = true)]
[Index("Email", Name = "UQ__Users__A9D105343D51E413", IsUnique = true)]
public partial class User
{
    [Key]
    public int UserId { get; set; }

    [StringLength(50)]
    public string Username { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [StringLength(100)]
    public string? FullName { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public int RoleId { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    public int? LoyaltyPoints { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<Pcbuild> Pcbuilds { get; set; } = new List<Pcbuild>();

    [InverseProperty("User")]
    public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual Role Role { get; set; } = null!;

    [InverseProperty("AssignedToUser")]
    public virtual ICollection<Ticket> TicketAssignedToUsers { get; set; } = new List<Ticket>();

    [InverseProperty("User")]
    public virtual ICollection<TicketComment> TicketComments { get; set; } = new List<TicketComment>();

    [InverseProperty("User")]
    public virtual ICollection<Ticket> TicketUsers { get; set; } = new List<Ticket>();
}
