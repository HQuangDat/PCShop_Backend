using PCShop_Backend.Dtos.OrderDtos;
using PCShop_Backend.Models;

namespace PCShop_Backend.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        // Receipts
        IQueryable<Receipt> QueryReceipts();
        Task<Receipt?> GetReceiptByIdAndUserAsync(int receiptId, int userId);
        Task AddReceiptAsync(Receipt receipt);
        void RemoveReceipt(Receipt receipt);

        // ReceiptItems
        IQueryable<ReceiptItem> QueryReceiptItems();
        Task<ReceiptItem?> GetReceiptItemAsync(int receiptId, int receiptItemId);
        Task AddReceiptItemsAsync(IEnumerable<ReceiptItem> items);
        void RemoveReceiptItem(ReceiptItem receiptItem);

        // Sales Statistics
        Task<List<SalesStatisticDto>> GetSalesStatisticsAsync(DateTime startDate, DateTime endDate);

        Task SaveChangesAsync();
    }
}
