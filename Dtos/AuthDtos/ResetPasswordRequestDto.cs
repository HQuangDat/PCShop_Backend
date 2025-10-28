
using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Dtos.AuthDtos
{
    public class ResetPasswordRequestDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string NewPassword { get; set; }
    }
}
