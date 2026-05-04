using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Models
{
    public class PasswordReset
    {
        public int Id { get; set; }

        [EmailAddress(ErrorMessage = "You must enter a valid email address!")]
        public required string Email { get; set; }
        public required string Token { get; set; }
        public DateTime ExpireDate { get; set; }
    }
}
