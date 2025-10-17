using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PCShop_Backend.Models;

[Index("CategoryName", Name = "UQ__Componen__8517B2E03F28D44D", IsUnique = true)]
public partial class ComponentCategory
{
    [Key]
    public int CategoryId { get; set; }

    [StringLength(50)]
    public string CategoryName { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<Component> Components { get; set; } = new List<Component>();
}
