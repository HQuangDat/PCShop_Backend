using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PCShop_Backend.Models;

[Index("CategoryId", Name = "IX_Components_CategoryId")]
[Index("IsActive", Name = "IX_Components_IsActive")]
[Index("Price", Name = "IX_Components_Price")]
public partial class Component
{
    [Key]
    public int ComponentId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    public int CategoryId { get; set; }

    [StringLength(50)]
    public string? Brand { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string? Description { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Component")]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [ForeignKey("CategoryId")]
    [InverseProperty("Components")]
    public virtual ComponentCategory Category { get; set; } = null!;

    [InverseProperty("Component")]
    public virtual ICollection<ComponentSpec> ComponentSpecs { get; set; } = new List<ComponentSpec>();

    [InverseProperty("Component")]
    public virtual ICollection<PcbuildComponent> PcbuildComponents { get; set; } = new List<PcbuildComponent>();

    [InverseProperty("Component")]
    public virtual ICollection<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;
}
