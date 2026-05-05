using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Data;
using PCShop_Backend.Models;
using PCShop_Backend.Repositories.Interfaces;

namespace PCShop_Backend.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<CartItem> QueryCartItems()
        {
            return _context.CartItems
                .Include(ci => ci.Build)
                .Include(ci => ci.Component);
        }

        public async Task<Component?> GetComponentByIdAsync(int? componentId)
        {
            return await _context.Components.FirstOrDefaultAsync(c => c.ComponentId == componentId);
        }

        public async Task<CartItem?> GetCartItemByIdAndUserAsync(int cartItemId, int userId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.UserId == userId);
        }

        public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId)
        {
            return await _context.CartItems.FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);
        }

        public async Task<List<CartItem>> GetUserCartItemsAsync(int userId)
        {
            return await _context.CartItems.Where(ci => ci.UserId == userId).ToListAsync();
        }

        public async Task AddCartItemAsync(CartItem cartItem)
        {
            await _context.CartItems.AddAsync(cartItem);
        }

        public void RemoveCartItem(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
        }

        public void RemoveCartItems(IEnumerable<CartItem> cartItems)
        {
            _context.CartItems.RemoveRange(cartItems);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
