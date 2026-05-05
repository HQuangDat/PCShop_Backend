using PCShop_Backend.Models;

namespace PCShop_Backend.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User existUser);
    }
}
