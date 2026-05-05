using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Data;
using PCShop_Backend.Dtos.OrderDtos;
using PCShop_Backend.Models;
using PCShop_Backend.Repositories.Interfaces;

namespace PCShop_Backend.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Receipt> QueryReceipts() => _context.Receipts;

        public async Task<Receipt?> GetReceiptByIdAndUserAsync(int receiptId, int userId)
        {
            return await _context.Receipts
                .FirstOrDefaultAsync(r => r.ReceiptId == receiptId && r.UserId == userId);
        }

        public async Task AddReceiptAsync(Receipt receipt)
        {
            await _context.Receipts.AddAsync(receipt);
        }

        public void RemoveReceipt(Receipt receipt)
        {
            _context.Receipts.Remove(receipt);
        }

        public IQueryable<ReceiptItem> QueryReceiptItems() => _context.ReceiptItems;

        public async Task<ReceiptItem?> GetReceiptItemAsync(int receiptId, int receiptItemId)
        {
            return await _context.ReceiptItems
                .Where(ri => ri.ReceiptId == receiptId)
                .FirstOrDefaultAsync(ri => ri.ReceiptItemId == receiptItemId);
        }

        public async Task AddReceiptItemsAsync(IEnumerable<ReceiptItem> items)
        {
            await _context.ReceiptItems.AddRangeAsync(items);
        }

        public void RemoveReceiptItem(ReceiptItem receiptItem)
        {
            _context.ReceiptItems.Remove(receiptItem);
        }

        public async Task<List<SalesStatisticDto>> GetSalesStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ReceiptItems
                .Where(ri => ri.Receipt.CreatedAt >= startDate && ri.Receipt.CreatedAt <= endDate)
                .Where(ri => ri.ComponentId.HasValue)
                .GroupBy(ri => new { ri.ComponentId, ri.Component!.Name })
                .Select(g => new SalesStatisticDto
                {
                    ProductId = g.Key.ComponentId!.Value,
                    ProductName = g.Key.Name ?? "Unknown",
                    TotalQuantitySold = g.Sum(ri => ri.Quantity),
                    TotalRevenue = g.Sum(ri => ri.Quantity * ri.UnitPrice),
                    Date = null
                })
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
