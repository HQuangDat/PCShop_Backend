using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PCShop_Backend.Models;

public partial class ReceiptItem
{
    [Key]
    public int ReceiptItemId { get; set; }

    public int ReceiptId { get; set; }

    public int? ComponentId { get; set; }

    public int? BuildId { get; set; }

    [StringLength(255)]
    public string ItemName { get; set; } = null!;

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; }

    [ForeignKey("BuildId")]
    [InverseProperty("ReceiptItems")]
    public virtual Pcbuild? Build { get; set; }

    [ForeignKey("ComponentId")]
    [InverseProperty("ReceiptItems")]
    public virtual Component? Component { get; set; }

    [ForeignKey("ReceiptId")]
    [InverseProperty("ReceiptItems")]
    public virtual Receipt Receipt { get; set; } = null!;
}
