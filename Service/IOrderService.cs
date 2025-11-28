using Gridify;
using PCShop_Backend.Dtos.OrderDtos;
using PCShop_Backend.Dtos.OrderDtos.CreateDtos;
using PCShop_Backend.Dtos.OrderDtos.UpdateDtos;

namespace PCShop_Backend.Service
{
    public interface IOrderService
    {
        //Receipts Section
        Task<Paging<ReceiptDtos>> getReceipts(GridifyQuery query);
        Task<Paging<ReceiptDtos>> getAllReceiptsByAdmin(GridifyQuery query);
        Task<ReceiptDtos> getReceiptById(int receiptId);
        Task CreateReceipt(CreateReceiptDto dto);
        Task UpdateReceipt(int receiptId, UpdateReceiptDto dto);
        Task DeleteReceipt(int receiptId);

        // Receipt Items Section
        Task<Paging<ReceiptItemsDto>> getReceiptItems(int receiptId,GridifyQuery query);
        Task<ReceiptItemsDto> GetReceiptItemById(int receiptId, int receiptItemId);
        Task CreateReceiptItem(int receiptId, CreateReceiptItemDto dto);
        Task UpdateReceiptItem(int receiptId,int receiptItemId, UpdateReceiptItemDto dto);
        Task DeleteReceiptItem(int receiptId, int receiptItemId);

        // Sales Statistics
        Task<List<SalesStatisticDto>> GetSalesStatistics(DateOnly startDate, DateOnly endDate);
    }
}
