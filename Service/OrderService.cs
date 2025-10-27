using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
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
        private readonly IDistributedCache _distributedCache;

        public OrderService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IDistributedCache distributedCache)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _distributedCache = distributedCache;
        }

        // ========== Cart Items ==========

        public async Task<Paging<CartItemsDtos>> getCartItems(GridifyQuery query)
        {
            var key = $"CartItems_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}".GetHashCode().ToString();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };
            //check if data is cached
            var cachedData = await _distributedCache.GetStringAsync(key);
            if(!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<Paging<CartItemsDtos>>(cachedData)!;
            }

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
        public async Task<Paging<ReceiptDtos>> getReceipts(GridifyQuery query)
        {
            var key = $"Receipts_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}".GetHashCode().ToString();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            var cachedData = await _distributedCache.GetStringAsync(key);
            if(!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<Paging<ReceiptDtos>>(cachedData)!;
            }

            var userIdClaim = int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            var receipts = await _context.Receipts
                .Where(r => r.UserId == userId)
                .Select(r => new ReceiptDtos
                {
                    ReceiptId = r.ReceiptId,
                    UserId = r.UserId,
                    TotalAmount = r.TotalAmount,
                    Status = r.Status,
                    PaymentMethod = r.PaymentMethod,
                    ShippingAddress = r.ShippingAddress,
                    City = r.City,
                    Country = r.Country,
                    TrackingNumber = r.TrackingNumber,
                    Notes = r.Notes,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .GridifyAsync(query);

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(receipts), options);

            return receipts;
        }
        public async Task<ReceiptDtos> getReceiptById(int receiptId)
        {
            var key = $"Receipt_{receiptId}".GetHashCode().ToString();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            var cachedData = await _distributedCache.GetStringAsync(key);
            if(!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<ReceiptDtos>(cachedData)!;
            }

            var userIdClaim = int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            var existingReceipt = await _context.Receipts
                .Where(r => r.ReceiptId == receiptId && r.UserId == userId)
                .Select(r => new ReceiptDtos
                {
                    ReceiptId = r.ReceiptId,
                    UserId = r.UserId,
                    TotalAmount = r.TotalAmount,
                    Status = r.Status,
                    PaymentMethod = r.PaymentMethod,
                    ShippingAddress = r.ShippingAddress,
                    City = r.City,
                    Country = r.Country,
                    TrackingNumber = r.TrackingNumber,
                    Notes = r.Notes,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .FirstOrDefaultAsync();
            if(existingReceipt == null)
            {
                throw new Exception("Receipt not found for the user.");
            }

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(existingReceipt), options);

            return existingReceipt!;
        }
        public async Task CreateReceipt(CreateReceiptDto dto)
        {
            var userIdClaim = int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            var newReceipt = new Receipt
            {
                UserId = userId,
                TotalAmount = dto.TotalAmount,
                Status = dto.Status,
                PaymentMethod = dto.PaymentMethod,
                ShippingAddress = dto.ShippingAddress,
                City = dto.City,
                Country = dto.Country,
                TrackingNumber = dto.TrackingNumber,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow            
            };
            await _context.Receipts.AddAsync(newReceipt);
            Log.Information($"User with id: {userId} has created a new receipt: {newReceipt.ReceiptId}");
            await _context.SaveChangesAsync();
        }
        public async Task UpdateReceipt(int receiptId, UpdateReceiptDto dto)
        {
            var userIdClaim = int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            var existingReceipt =  _context.Receipts.FirstOrDefault(r => r.ReceiptId == receiptId && r.UserId == userId);
            if(existingReceipt == null)
            {
                throw new Exception("Receipt not found for the user.");
            }
            existingReceipt.TotalAmount = dto.TotalAmount;
            existingReceipt.Status = dto.Status;
            existingReceipt.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var key = $"Receipt_{receiptId}".GetHashCode().ToString();
            await _distributedCache.RemoveAsync(key);
        }

        public async Task DeleteReceipt(int receiptId)
        {
            var userIdClaim = int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            var existingReceipt = await _context.Receipts.FirstOrDefaultAsync(r => r.ReceiptId == receiptId && r.UserId == userId);
            if (existingReceipt == null)
            {
                throw new Exception("Receipt not found for the user.");
            }
            _context.Receipts.Remove(existingReceipt);
            Log.Information($"User with id: {userId} has deleted receipt: {existingReceipt.ReceiptId}");
            await _context.SaveChangesAsync();

            var key = $"Receipt_{receiptId}".GetHashCode().ToString();
            await _distributedCache.RemoveAsync(key);
        }

        // ========== Receipt Items Section ==========
        public async Task<Paging<ReceiptItemsDto>> getReceiptItems(GridifyQuery query)
        {
            var key = $"ReceiptItems_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}".GetHashCode().ToString();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            var cachedData = await _distributedCache.GetStringAsync(key);
            if(!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<Paging<ReceiptItemsDto>>(cachedData)!;
            }

            var receiptItems = await _context.ReceiptItems
                .Select(ri => new ReceiptItemsDto
                {
                    ReceiptItemId = ri.ReceiptItemId,
                    ReceiptId = ri.ReceiptId,
                    ComponentId = ri.ComponentId,
                    BuildId = ri.BuildId,
                    ItemName = ri.ItemName,
                    Quantity = ri.Quantity,
                    UnitPrice = ri.UnitPrice
                })
                .GridifyAsync(query);

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(receiptItems), options);

            return receiptItems;
        }
        public async Task<ReceiptItemsDto> GetReceiptItemById(int receiptItemId)
        {
            var key = $"ReceiptItem_{receiptItemId}".GetHashCode().ToString();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            var cachedData = await _distributedCache.GetStringAsync(key);
            if(!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<ReceiptItemsDto>(cachedData)!;
            }

            var existingReceiptItem = await _context.ReceiptItems
                .Where(ri => ri.ReceiptItemId == receiptItemId)
                .Select(ri => new ReceiptItemsDto
                {
                    ReceiptItemId = ri.ReceiptItemId,
                    ReceiptId = ri.ReceiptId,
                    ComponentId = ri.ComponentId,
                    BuildId = ri.BuildId,
                    ItemName = ri.ItemName,
                    Quantity = ri.Quantity,
                    UnitPrice = ri.UnitPrice
                })
                .FirstOrDefaultAsync();
            if (existingReceiptItem == null)
            {
                throw new Exception("Receipt item not found.");
            }

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(existingReceiptItem), options);

            return existingReceiptItem!;
        }

        public async Task CreateReceiptItem(CreateReceiptItemDto dto)
        {
            var newReceiptItem = new ReceiptItem
            {
                ReceiptId = dto.ReceiptId,
                ComponentId = dto.ComponentId,
                BuildId = dto.BuildId,
                ItemName = dto.ItemName,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice
            };
            await _context.ReceiptItems.AddAsync(newReceiptItem);
            Log.Information($"A new receipt item with id: {newReceiptItem.ReceiptItemId} has been created");
            await _context.SaveChangesAsync();
        }

        public async Task UpdateReceiptItem(int receiptItemId, UpdateReceiptItemDto dto)
        {
            var existingReceiptItem = await _context.ReceiptItems.FirstOrDefaultAsync(ri => ri.ReceiptItemId == receiptItemId);
            if (existingReceiptItem == null)
            {
                throw new Exception("Receipt item not found.");
            }
            existingReceiptItem.ReceiptId = dto.ReceiptId;
            existingReceiptItem.ComponentId = dto.ComponentId;
            existingReceiptItem.BuildId = dto.BuildId;
            existingReceiptItem.ItemName = dto.ItemName;
            existingReceiptItem.Quantity = dto.Quantity;
            existingReceiptItem.UnitPrice = dto.UnitPrice;
            Log.Information($"Receipt item with id: {existingReceiptItem.ReceiptItemId} has been updated");
            await _context.SaveChangesAsync();

            var key = $"ReceiptItem_{receiptItemId}".GetHashCode().ToString();
            await _distributedCache.RemoveAsync(key);
        }
        public async Task DeleteReceiptItem(int receiptItemId)
        {
            var existingReceiptItem = await _context.ReceiptItems.FirstOrDefaultAsync(ri => ri.ReceiptItemId == receiptItemId);
            if (existingReceiptItem == null)
            {
                throw new Exception("Receipt item not found.");
            }
            _context.ReceiptItems.Remove(existingReceiptItem);
            Log.Information($"Receipt item with id: {existingReceiptItem.ReceiptItemId} has been deleted");
            await _context.SaveChangesAsync();

            var key = $"ReceiptItem_{receiptItemId}".GetHashCode().ToString();
            await _distributedCache.RemoveAsync(key);
        }
    }
}
