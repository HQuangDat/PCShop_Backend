using PCShop_Backend.Data;

namespace PCShop_Backend.Service
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
