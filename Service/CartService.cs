using Gridify;
using Gridify.EntityFramework;
using PCShop_Backend.Dtos.CartDtos;
using PCShop_Backend.Dtos.CartDtos.CreateDtos;
using PCShop_Backend.Dtos.CartDtos.UpdateDtos;
using PCShop_Backend.Exceptions;
using PCShop_Backend.Interfaces;
using PCShop_Backend.Models;
using PCShop_Backend.Repositories.Interfaces;
using Serilog;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PCShop_Backend.Service
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheService _cacheService;

        public CartService(ICartRepository cartRepository, IHttpContextAccessor httpContextAccessor, ICacheService cacheService)
        {
            _cartRepository = cartRepository;
            _httpContextAccessor = httpContextAccessor;
            _cacheService = cacheService;
        }

        public async Task<Paging<CartItemsDtos>> getCartItems(GridifyQuery query)
        {
            int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

            var rawKey = $"CartItems_{userId}_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<CartItemsDtos>>(key);
            if (cachedData != null)
                return cachedData;

            var result = await _cartRepository.QueryCartItems()
                .Where(ci => ci.UserId == userId)
                .Select(ci => new CartItemsDtos
                {
                    CartItemId = ci.CartItemId,
                    UserId = ci.UserId,
                    ComponentId = ci.ComponentId,
                    BuildId = ci.BuildId,
                    Quantity = ci.Quantity,
                    AddedAt = ci.AddedAt
                }).GridifyAsync(query);

            await _cacheService.SetAsync(key, result);
            return result;
        }

        public async Task AddToCart(AddItemToCartDtos dto)
        {
            int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

            var component = await _cartRepository.GetComponentByIdAsync(dto.ComponentId);
            if (component == null)
                throw new NotFoundException("Component not found.");

            if (component.StockQuantity < dto.Quantity)
                throw new OutOfStockException("Not enough stock for the requested component.");

            var addItem = new CartItem
            {
                UserId = userId,
                ComponentId = dto.ComponentId,
                BuildId = dto.BuildId,
                Quantity = dto.Quantity,
                AddedAt = DateTime.UtcNow
            };

            await _cartRepository.AddCartItemAsync(addItem);
            await _cartRepository.SaveChangesAsync();
            Log.Information("User {UserId} added item to cart", userId);
        }

        public async Task UpdateCartItems(int cartItemId, UpdateCartItemsDto dto)
        {
            int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

            var existingCartItem = await _cartRepository.GetCartItemByIdAndUserAsync(cartItemId, userId);
            if (existingCartItem == null)
                throw new NotFoundException("Cart item not found for the user.");

            existingCartItem.Quantity = dto.Quantity;
            await _cartRepository.SaveChangesAsync();
            Log.Information("User {UserId} updated cart item {CartItemId}", userId, existingCartItem.CartItemId);
        }

        public async Task RemoveFromCart(int cartItemId)
        {
            var existingCartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);
            if (existingCartItem == null)
                throw new NotFoundException("Cart item not found.");

            _cartRepository.RemoveCartItem(existingCartItem);
            await _cartRepository.SaveChangesAsync();
            Log.Information("Cart item {CartItemId} removed from cart", existingCartItem.CartItemId);
        }

        public async Task ClearCart()
        {
            int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

            var userCartItems = await _cartRepository.GetUserCartItemsAsync(userId);
            _cartRepository.RemoveCartItems(userCartItems);
            await _cartRepository.SaveChangesAsync();
            Log.Information("User {UserId} cleared their cart", userId);
        }
    }
}
