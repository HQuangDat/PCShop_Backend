using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace PCShop_Backend.Models;

[Index("SpecKey", Name = "IX_ComponentSpecs_SpecKey")]
[Index("ComponentId", "SpecKey", Name = "UQ_Component_Spec", IsUnique = true)]
public partial class ComponentSpec
{
    [Key]
    public int SpecId { get; set; }

    public int ComponentId { get; set; }

    [StringLength(50)]
    public string SpecKey { get; set; } = null!;

    [StringLength(255)]
    public string SpecValue { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    [ForeignKey("ComponentId")]
    [InverseProperty("ComponentSpecs")]
    [JsonIgnore]
    public virtual Component Component { get; set; } = null!;
}
