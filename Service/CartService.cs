using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using PCShop_Backend.Data;
using PCShop_Backend.Dtos.CartDtos;
using PCShop_Backend.Dtos.CartDtos.CreateDtos;
using PCShop_Backend.Dtos.CartDtos.UpdateDtos;
using PCShop_Backend.Exceptions;
using PCShop_Backend.Models;
using Serilog;
using System.Security.Claims;

namespace PCShop_Backend.Service
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _distributedCache;

        public CartService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IDistributedCache distributedCache)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _distributedCache = distributedCache;
        }

        // ========== Cart Items ==========

        public async Task<Paging<CartItemsDtos>> getCartItems(GridifyQuery query)
        {
            var userIdClaim = int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            var key = $"CartItems_{userId}_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}".GetHashCode().ToString();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };
            //check if data is cached
            var cachedData = await _distributedCache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<Paging<CartItemsDtos>>(cachedData)!;
            }

            var cartitems = await _context.CartItems
                .Include(pcb => pcb.Build)
                .Include(ci => ci.Component)
                .Select(ci => new CartItemsDtos
                {
                    CartItemId = ci.CartItemId,
                    UserId = ci.UserId,
                    ComponentId = ci.ComponentId,
                    BuildId = ci.BuildId,
                    Quantity = ci.Quantity,
                    AddedAt = ci.AddedAt
                }).Where(ci => ci.UserId == userId)
                .GridifyAsync(query);

            //cache the data
            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(cartitems), options);

            return cartitems;
        }

        public async Task AddToCart(AddItemToCartDtos dto)
        {
            var userIdClaim = int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

            var component = await _context.Components.FirstOrDefaultAsync(c => c.ComponentId == dto.ComponentId);

            // Check stock availability
            if (component!.StockQuantity < dto.Quantity)
            {
                throw new OutOfStockException("Not enough stock for the requested component.");
            }

            var addItem = new CartItem
            {
                UserId = userId,
                ComponentId = dto.ComponentId,
                BuildId = dto.BuildId,
                Quantity = dto.Quantity,
                AddedAt = DateTime.UtcNow
            };

            await _context.CartItems.AddAsync(addItem);
            Log.Information($"User with id: {userId} has added item: {addItem.CartItemId} to cart");
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartItems(int cartItemId, UpdateCartItemsDto dto)
        {
            var userIdClaim = int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            var existingCartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.UserId == userId);
            if (existingCartItem == null)
            {
                throw new NotFoundException("Cart item not found for the user.");
            }
            existingCartItem.Quantity = dto.Quantity;
            await _context.SaveChangesAsync();
            Log.Information($"User with id: {userId} has updated Cart: {existingCartItem.CartItemId}");
        }

        public async Task RemoveFromCart(int cartItemId)
        {
            var existingCartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);
            if (existingCartItem == null)
            {
                throw new NotFoundException("Cart item not found.");
            }
            _context.CartItems.Remove(existingCartItem);
            Log.Information($"Cart item with id: {existingCartItem.CartItemId} has been removed from cart");
            await _context.SaveChangesAsync();
        }

        public async Task ClearCart()
        {
            var userIdClaim = int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            var userCartItems = await _context.CartItems.Where(ci => ci.UserId == userId).ToListAsync();
            _context.CartItems.RemoveRange(userCartItems);
            Log.Information($"User with id: {userId} has cleared their cart");
            await _context.SaveChangesAsync();
        }
    }
}
