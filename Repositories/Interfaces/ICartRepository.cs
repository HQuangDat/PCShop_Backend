using PCShop_Backend.Models;

namespace PCShop_Backend.Repositories.Interfaces
{
    public interface ICartRepository
    {
        IQueryable<CartItem> QueryCartItems();
        Task<Component?> GetComponentByIdAsync(int? componentId);
        Task<CartItem?> GetCartItemByIdAndUserAsync(int cartItemId, int userId);
        Task<CartItem?> GetCartItemByIdAsync(int cartItemId);
        Task<List<CartItem>> GetUserCartItemsAsync(int userId);
        Task AddCartItemAsync(CartItem cartItem);
        void RemoveCartItem(CartItem cartItem);
        void RemoveCartItems(IEnumerable<CartItem> cartItems);
        Task SaveChangesAsync();
    }
}
