using PCShop_Backend.Data;

namespace PCShop_Backend.Service
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
