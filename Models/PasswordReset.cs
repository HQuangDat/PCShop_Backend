using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Models
{
    public class PasswordReset
    {
        public int Id { get; set; }

        [EmailAddress(ErrorMessage = "You must enter a valid email address!")]
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime ExpireDate { get; set; }
    }
}
