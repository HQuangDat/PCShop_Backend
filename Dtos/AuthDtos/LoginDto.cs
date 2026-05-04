using System.Text.Json.Serialization;

namespace PCShop_Backend.Dtos.AuthDtos
{
    public class LoginDto
    {
        public required string username { get; set; }
        public required string password { get; set; }
    }
}
