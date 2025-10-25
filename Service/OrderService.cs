using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Data;
using PCShop_Backend.Dtos;
using PCShop_Backend.Dtos.OrderDtos;
using PCShop_Backend.Dtos.OrderDtos.CreateDtos;
using PCShop_Backend.Dtos.OrderDtos.UpdateDtos;
using PCShop_Backend.Models;
using Serilog;
using System.Security.Claims;

namespace PCShop_Backend.Service
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // ========== Cart Items ==========

        public async Task<Paging<CartItemsDtos>> getCartItems(GridifyQuery query)
        {
            var cartitems = await _context.CartItems
                .Include(pcb=>pcb.Build)
                .Include(ci => ci.Component)
                .Select(ci => new CartItemsDtos
                {
                    CartItemId = ci.CartItemId,
                    UserId = ci.UserId,
                    ComponentId = ci.ComponentId,
                    BuildId = ci.BuildId,
                    Quantity = ci.Quantity,
                    AddedAt = ci.AddedAt
                })
                .GridifyAsync(query);
            return cartitems;
        }

        public async Task AddToCart(AddItemToCartDtos dto)
        {
            var userIdClaim = int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            
            var component = await _context.Components.FirstOrDefaultAsync(c => c.ComponentId == dto.ComponentId);

            // Check stock availability
            if (component!.StockQuantity < dto.Quantity)
            {
                throw new Exception("Not enough stock for the requested component.");
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

        public async Task UpdateCartItems(int cartItemId,UpdateCartItemsDto dto)
        {
            var userIdClaim = int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            var existingCartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.UserId == userId);
            if(existingCartItem == null)
            {
                throw new Exception("Cart item not found for the user.");
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
                throw new Exception("Cart item not found.");
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


        // ========== Receipts Section ==========
        public Task<Paging<ReceiptDtos>> getReceipts(GridifyQuery query)
        {
            throw new NotImplementedException();
        }
        public Task<ReceiptDtos> getReceiptDetails(int receiptId)
        {
            throw new NotImplementedException();
        }
        public Task CreateReceipt(CreateReceiptDto dto)
        {
            throw new NotImplementedException();
        }
        public Task UpdateReceipt(int receiptId, UpdateReceiptDto dto)
        {
            throw new NotImplementedException();
        }

        public Task DeleteReceipt(int receiptId)
        {
            throw new NotImplementedException();
        }

        // ========== Receipt Items Section ==========
        public Task<Paging<ReceiptItemsDto>> getReceiptItems(GridifyQuery query)
        {
            throw new NotImplementedException();
        }
        public Task<ReceiptItemsDto> GetReceiptItemById(int receiptItemId)
        {
            throw new NotImplementedException();
        }

        public Task CreateReceiptItem(CreateReceiptItemDto dto)
        {
            throw new NotImplementedException();
        }

        public Task UpdateReceiptItem(int receiptItemId, UpdateReceiptItemDto dto)
        {
            throw new NotImplementedException();
        }
        public Task DeleteReceiptItem(int receiptItemId)
        {
            throw new NotImplementedException();
        }
    }
}
