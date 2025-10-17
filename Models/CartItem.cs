using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PCShop_Backend.Models;

public partial class CartItem
{
    [Key]
    public int CartItemId { get; set; }

    public int UserId { get; set; }

    public int? ComponentId { get; set; }

    public int? BuildId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? AddedAt { get; set; }

    [ForeignKey("BuildId")]
    [InverseProperty("CartItems")]
    public virtual Pcbuild? Build { get; set; }

    [ForeignKey("ComponentId")]
    [InverseProperty("CartItems")]
    public virtual Component? Component { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("CartItems")]
    public virtual User User { get; set; } = null!;
}
