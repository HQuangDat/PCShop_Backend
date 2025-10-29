using PCShop_Backend.Models;

namespace PCShop_Backend.Service
{
    public interface IJwtTokenService
    {
        string GenerateToken(User existUser);
    }
}
