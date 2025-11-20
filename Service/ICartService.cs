using Gridify;
using PCShop_Backend.Dtos.CartDtos;
using PCShop_Backend.Dtos.CartDtos.CreateDtos;
using PCShop_Backend.Dtos.CartDtos.UpdateDtos;

namespace PCShop_Backend.Service
{
    public interface ICartService
    {
        // Cart Items
        Task<Paging<CartItemsDtos>> getCartItems(GridifyQuery query);

        Task AddToCart(AddItemToCartDtos dto);
        Task UpdateCartItems(int cartId, UpdateCartItemsDto dto);
        Task RemoveFromCart(int cartItemId);

        Task ClearCart();
    }
}
