using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Dtos.UserDtos.UpdateDto
{
    public class UpdateUserDto
    {
        [StringLength(100, MinimumLength = 1)]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }
    }
}
