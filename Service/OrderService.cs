using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Data;
using PCShop_Backend.Dtos.OrderDtos;
using PCShop_Backend.Dtos.OrderDtos.CreateDtos;
using PCShop_Backend.Dtos.OrderDtos.UpdateDtos;
using PCShop_Backend.Exceptions;
using PCShop_Backend.Models;
using Serilog;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PCShop_Backend.Service
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheService _cacheService;

        public OrderService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, ICacheService cacheService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _cacheService = cacheService;
        }


        // ========== Receipts Section ==========
        public async Task<Paging<ReceiptDtos>> getReceipts(GridifyQuery query)
        {
            var userIdClaim = int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            
            var rawKey = $"Receipts_{userId}_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<ReceiptDtos>>(key);
            if (cachedData != null)
            {
                return cachedData;
            }

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

            await _cacheService.SetAsync(key, receipts);

            return receipts;
        }
        public async Task<Paging<ReceiptDtos>> getAllReceiptsByAdmin(GridifyQuery query)
        {
            var rawKey = $"Receipts_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<ReceiptDtos>>(key);
            if (cachedData != null)
            {
                return cachedData;
            }

            var receipts = await _context.Receipts
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

            await _cacheService.SetAsync(key, receipts);

            return receipts;
        }
        public async Task<ReceiptDtos> getReceiptById(int receiptId)
        {
            var userIdClaim = int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

            var rawKey = $"Receipt_{userId}_{receiptId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<ReceiptDtos>(key);
            if (cachedData != null)
            {
                return cachedData;
            }

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
                throw new NotFoundException("Receipt not found for the user.");
            }

            await _cacheService.SetAsync(key, existingReceipt);

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
                throw new NotFoundException("Receipt not found for the user.");
            }
            existingReceipt.TotalAmount = dto.TotalAmount;
            existingReceipt.Status = dto.Status;
            existingReceipt.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var rawKey = $"Receipt_{userId}_{receiptId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(key);
        }

        public async Task DeleteReceipt(int receiptId)
        {
            var userIdClaim = int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            var existingReceipt = await _context.Receipts.FirstOrDefaultAsync(r => r.ReceiptId == receiptId && r.UserId == userId);
            if (existingReceipt == null)
            {
                throw new NotFoundException("Receipt not found for the user.");
            }
            _context.Receipts.Remove(existingReceipt);
            Log.Information($"User with id: {userId} has deleted receipt: {existingReceipt.ReceiptId}");
            await _context.SaveChangesAsync();

            var rawKey = $"Receipt_{userId}_{receiptId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(key);
        }

        // ========== Receipt Items Section ==========
        public async Task<Paging<ReceiptItemsDto>> getReceiptItems(int receiptId, GridifyQuery query)
        {
            var rawKey = $"ReceiptItems_{receiptId}_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<ReceiptItemsDto>>(key);
            if (cachedData != null)
            {
                return cachedData;
            }

            var receiptItems = await _context.ReceiptItems
                .Where(ri => ri.ReceiptId == receiptId)
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

            await _cacheService.SetAsync(key, receiptItems);

            return receiptItems;
        }
        public async Task<ReceiptItemsDto> GetReceiptItemById(int receiptId, int receiptItemId)
        {
            var rawKey = $"ReceiptItem_{receiptItemId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<ReceiptItemsDto>(key);
            if (cachedData != null)
            {
                return cachedData;
            }

            var existingReceiptItem = await _context.ReceiptItems
                .Where(ri => ri.ReceiptItemId == receiptItemId && ri.ReceiptId == receiptId)
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
                throw new NotFoundException("Receipt item not found.");
            }

            await _cacheService.SetAsync(key, existingReceiptItem);

            return existingReceiptItem!;
        }

        public async Task CreateReceiptItem(int receiptId, CreateReceiptItemDto dto)
        {
            var newReceiptItems = new List<ReceiptItem>
            {
                new ReceiptItem
                {
                    ReceiptId = receiptId,
                    ComponentId = dto.ComponentId,
                    BuildId = dto.BuildId,
                    ItemName = dto.ItemName,
                    Quantity = dto.Quantity,
                    UnitPrice = dto.UnitPrice
                }
            };
            await _context.ReceiptItems.AddRangeAsync(newReceiptItems);
            Log.Information($"A new receipt items has been created");
            await _context.SaveChangesAsync();
        }

        public async Task UpdateReceiptItem(int receiptId, int receiptItemId, UpdateReceiptItemDto dto)
        {
            var existingReceiptItem = await _context.ReceiptItems.Where(ri=>ri.ReceiptId == receiptId).FirstOrDefaultAsync(ri => ri.ReceiptItemId == receiptItemId);
            if (existingReceiptItem == null)
            {
                throw new NotFoundException("Receipt item not found.");
            }
            existingReceiptItem.ReceiptId = dto.ReceiptId;
            existingReceiptItem.ComponentId = dto.ComponentId;
            existingReceiptItem.BuildId = dto.BuildId;
            existingReceiptItem.ItemName = dto.ItemName;
            existingReceiptItem.Quantity = dto.Quantity;
            existingReceiptItem.UnitPrice = dto.UnitPrice;
            Log.Information($"Receipt item with id: {existingReceiptItem.ReceiptItemId} has been updated");
            await _context.SaveChangesAsync();

            var rawKey = $"ReceiptItem_{receiptItemId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(key);
        }
        public async Task DeleteReceiptItem(int receiptId, int receiptItemId)
        {
            var existingReceiptItem = await _context.ReceiptItems
                .Where(ri => ri.ReceiptId == receiptId)
                .FirstOrDefaultAsync(ri => ri.ReceiptItemId == receiptItemId);
            if (existingReceiptItem == null)
            {
                throw new NotFoundException("Receipt item not found.");
            }
            _context.ReceiptItems.Remove(existingReceiptItem);
            Log.Information($"Receipt item with id: {existingReceiptItem.ReceiptItemId} has been deleted");
            await _context.SaveChangesAsync();

            var rawKey = $"ReceiptItem_{receiptItemId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(key);
        }

        // ========== Sales Statistics ==========
        public async Task<List<SalesStatisticDto>> GetSalesStatistics(DateOnly startDate, DateOnly endDate)
        {
            var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
            var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);

            // Query kiem tra ReceiptItems trong khoang thoi gian
            var salesStats = await _context.ReceiptItems
                .Where(ri => ri.Receipt.CreatedAt >= startDateTime && ri.Receipt.CreatedAt <= endDateTime)
                //Kiem tra ComponentId khac null
                .Where(ri => ri.ComponentId.HasValue)
                .GroupBy(ri => new { ri.ComponentId, ri.Component.Name }) 
                .Select(g => new SalesStatisticDto
                {
                    ProductId = g.Key.ComponentId.Value,
                    ProductName = g.Key.Name ?? "Unknown",
                    TotalQuantitySold = g.Sum(ri => ri.Quantity),
                    TotalRevenue = g.Sum(ri => ri.Quantity * ri.UnitPrice),
                    Date = null 
                })
                .ToListAsync();

            if (!salesStats.Any())
            {
                Log.Information("No receipts found in the given date range.");
                throw new NotFoundException("No receipts found in the given date range.");
            }
            return salesStats;
        }
    }
}
