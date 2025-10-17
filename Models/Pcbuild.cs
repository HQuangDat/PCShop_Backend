using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PCShop_Backend.Models;

[Table("PCBuilds")]
public partial class Pcbuild
{
    [Key]
    public int BuildId { get; set; }

    [StringLength(100)]
    public string BuildName { get; set; } = null!;

    public string? Description { get; set; }

    public int? CreatedByUserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    public bool? IsPublic { get; set; }

    [InverseProperty("Build")]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("Pcbuilds")]
    public virtual User? CreatedByUser { get; set; }

    [InverseProperty("Build")]
    public virtual ICollection<PcbuildComponent> PcbuildComponents { get; set; } = new List<PcbuildComponent>();

    [InverseProperty("Build")]
    public virtual ICollection<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();
}
