using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PCShop_Backend.Models;

[Table("PCBuildComponents")]
[Index("BuildId", "ComponentId", Name = "UQ_Build_Component", IsUnique = true)]
public partial class PcbuildComponent
{
    [Key]
    public int BuildComponentId { get; set; }

    public int BuildId { get; set; }

    public int ComponentId { get; set; }

    public int Quantity { get; set; }

    [ForeignKey("BuildId")]
    [InverseProperty("PcbuildComponents")]
    public virtual Pcbuild Build { get; set; } = null!;

    [ForeignKey("ComponentId")]
    [InverseProperty("PcbuildComponents")]
    public virtual Component Component { get; set; } = null!;
}
