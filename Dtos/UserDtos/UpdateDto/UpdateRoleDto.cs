using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Dtos.UserDtos.UpdateDto
{
    public class UpdateRoleDto
    {
        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Role name must be between 1 and 100 characters.")]
        public string RoleName { get; set; }

        [StringLength(255)]
        public string Description { get; set; }
    }
}
