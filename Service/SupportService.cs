using PCShop_Backend.Data;

namespace PCShop_Backend.Service
{
    public class SupportService : ISupportService
    {
        private readonly ApplicationDbContext _context;

        public SupportService(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
