using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Dtos.OrderDtos;
using PCShop_Backend.Dtos.OrderDtos.CreateDtos;
using PCShop_Backend.Dtos.OrderDtos.UpdateDtos;
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
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheService _cacheService;

        public OrderService(IOrderRepository orderRepository, IHttpContextAccessor httpContextAccessor, ICacheService cacheService)
        {
            _orderRepository = orderRepository;
            _httpContextAccessor = httpContextAccessor;
            _cacheService = cacheService;
        }

        // ========== Receipts ==========
        public async Task<Paging<ReceiptDtos>> getReceipts(GridifyQuery query)
        {
            int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

            var rawKey = $"Receipts_{userId}_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<ReceiptDtos>>(key);
            if (cachedData != null)
                return cachedData;

            var result = await _orderRepository.QueryReceipts()
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
                }).GridifyAsync(query);

            await _cacheService.SetAsync(key, result);
            return result;
        }

        public async Task<Paging<ReceiptDtos>> getAllReceiptsByAdmin(GridifyQuery query)
        {
            var rawKey = $"Receipts_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<ReceiptDtos>>(key);
            if (cachedData != null)
                return cachedData;

            var result = await _orderRepository.QueryReceipts()
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
                }).GridifyAsync(query);

            await _cacheService.SetAsync(key, result);
            return result;
        }

        public async Task<ReceiptDtos> getReceiptById(int receiptId)
        {
            int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

            var rawKey = $"Receipt_{userId}_{receiptId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<ReceiptDtos>(key);
            if (cachedData != null)
                return cachedData;

            var existingReceipt = await _orderRepository.QueryReceipts()
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

            if (existingReceipt == null)
                throw new NotFoundException("Receipt not found for the user.");

            await _cacheService.SetAsync(key, existingReceipt);
            return existingReceipt;
        }

        public async Task CreateReceipt(CreateReceiptDto dto)
        {
            int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

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
            await _orderRepository.AddReceiptAsync(newReceipt);
            await _orderRepository.SaveChangesAsync();
            Log.Information("User {UserId} created receipt {ReceiptId}", userId, newReceipt.ReceiptId);
        }

        public async Task UpdateReceipt(int receiptId, UpdateReceiptDto dto)
        {
            int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

            var existingReceipt = await _orderRepository.GetReceiptByIdAndUserAsync(receiptId, userId);
            if (existingReceipt == null)
                throw new NotFoundException("Receipt not found for the user.");

            existingReceipt.TotalAmount = dto.TotalAmount;
            existingReceipt.Status = dto.Status;
            existingReceipt.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.SaveChangesAsync();

            var rawKey = $"Receipt_{userId}_{receiptId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        public async Task DeleteReceipt(int receiptId)
        {
            int.TryParse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);

            var existingReceipt = await _orderRepository.GetReceiptByIdAndUserAsync(receiptId, userId);
            if (existingReceipt == null)
                throw new NotFoundException("Receipt not found for the user.");

            _orderRepository.RemoveReceipt(existingReceipt);
            await _orderRepository.SaveChangesAsync();
            Log.Information("User {UserId} deleted receipt {ReceiptId}", userId, existingReceipt.ReceiptId);

            var rawKey = $"Receipt_{userId}_{receiptId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        // ========== Receipt Items ==========
        public async Task<Paging<ReceiptItemsDto>> getReceiptItems(int receiptId, GridifyQuery query)
        {
            var rawKey = $"ReceiptItems_{receiptId}_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<ReceiptItemsDto>>(key);
            if (cachedData != null)
                return cachedData;

            var result = await _orderRepository.QueryReceiptItems()
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
                }).GridifyAsync(query);

            await _cacheService.SetAsync(key, result);
            return result;
        }

        public async Task<ReceiptItemsDto> GetReceiptItemById(int receiptId, int receiptItemId)
        {
            var rawKey = $"ReceiptItem_{receiptItemId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<ReceiptItemsDto>(key);
            if (cachedData != null)
                return cachedData;

            var existingReceiptItem = await _orderRepository.QueryReceiptItems()
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
                throw new NotFoundException("Receipt item not found.");

            await _cacheService.SetAsync(key, existingReceiptItem);
            return existingReceiptItem;
        }

        public async Task CreateReceiptItem(int receiptId, CreateReceiptItemDto dto)
        {
            await _orderRepository.AddReceiptItemsAsync(new[]
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
            });
            await _orderRepository.SaveChangesAsync();
            Log.Information("Receipt item created for receipt {ReceiptId}", receiptId);
        }

        public async Task UpdateReceiptItem(int receiptId, int receiptItemId, UpdateReceiptItemDto dto)
        {
            var existingReceiptItem = await _orderRepository.GetReceiptItemAsync(receiptId, receiptItemId);
            if (existingReceiptItem == null)
                throw new NotFoundException("Receipt item not found.");

            existingReceiptItem.ReceiptId = dto.ReceiptId;
            existingReceiptItem.ComponentId = dto.ComponentId;
            existingReceiptItem.BuildId = dto.BuildId;
            existingReceiptItem.ItemName = dto.ItemName;
            existingReceiptItem.Quantity = dto.Quantity;
            existingReceiptItem.UnitPrice = dto.UnitPrice;
            await _orderRepository.SaveChangesAsync();
            Log.Information("Receipt item {ReceiptItemId} updated", existingReceiptItem.ReceiptItemId);

            var rawKey = $"ReceiptItem_{receiptItemId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        public async Task DeleteReceiptItem(int receiptId, int receiptItemId)
        {
            var existingReceiptItem = await _orderRepository.GetReceiptItemAsync(receiptId, receiptItemId);
            if (existingReceiptItem == null)
                throw new NotFoundException("Receipt item not found.");

            _orderRepository.RemoveReceiptItem(existingReceiptItem);
            await _orderRepository.SaveChangesAsync();
            Log.Information("Receipt item {ReceiptItemId} deleted", existingReceiptItem.ReceiptItemId);

            var rawKey = $"ReceiptItem_{receiptItemId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        // ========== Sales Statistics ==========
        public async Task<List<SalesStatisticDto>> GetSalesStatistics(DateOnly startDate, DateOnly endDate)
        {
            var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
            var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);

            var salesStats = await _orderRepository.GetSalesStatisticsAsync(startDateTime, endDateTime);

            if (!salesStats.Any())
            {
                Log.Information("No receipts found in the given date range.");
                throw new NotFoundException("No receipts found in the given date range.");
            }

            return salesStats;
        }
    }
}
