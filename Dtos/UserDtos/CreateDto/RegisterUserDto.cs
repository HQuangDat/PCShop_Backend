using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PCShop_Backend.Dtos.UserDtos.CreateDto
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, MinimumLength = 1)]
        [JsonPropertyName("fullName")]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }

        [StringLength(255)]
        [JsonPropertyName("address")]
        public string Address { get; set; }

        [StringLength(100)]
        [JsonPropertyName("city")]
        public string City { get; set; }

        [StringLength(100)]
        [JsonPropertyName("country")]
        public string Country { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
