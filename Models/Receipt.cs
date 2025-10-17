using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PCShop_Backend.Models;

[Index("CreatedAt", Name = "IX_Receipts_CreatedAt")]
[Index("Status", Name = "IX_Receipts_Status")]
[Index("UserId", Name = "IX_Receipts_UserId")]
public partial class Receipt
{
    [Key]
    public int ReceiptId { get; set; }

    public int UserId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [StringLength(255)]
    public string? ShippingAddress { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(100)]
    public string? TrackingNumber { get; set; }

    public string? Notes { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Receipt")]
    public virtual ICollection<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();

    [ForeignKey("UserId")]
    [InverseProperty("Receipts")]
    public virtual User User { get; set; } = null!;
}
