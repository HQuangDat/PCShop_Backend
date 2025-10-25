using Gridify;
using PCShop_Backend.Dtos.OrderDtos;
using PCShop_Backend.Dtos.OrderDtos.CreateDtos;
using PCShop_Backend.Dtos.OrderDtos.UpdateDtos;

namespace PCShop_Backend.Service
{
    public interface IOrderService
    {

        // Cart Items
        Task<Paging<CartItemsDtos>> getCartItems(GridifyQuery query);

        Task AddToCart(AddItemToCartDtos dto);
        Task UpdateCartItems(int cartId,UpdateCartItemsDto dto);
        Task RemoveFromCart(int cartItemId);

        Task ClearCart();

        //Receipts Section
        Task<Paging<ReceiptDtos>> getReceipts(GridifyQuery query);
        Task<ReceiptDtos> getReceiptById(int receiptId);
        Task CreateReceipt(CreateReceiptDto dto);
        Task UpdateReceipt(int receiptId, UpdateReceiptDto dto);
        Task DeleteReceipt(int receiptId);

        // Receipt Items Section
        Task<Paging<ReceiptItemsDto>> getReceiptItems(GridifyQuery query);
        Task<ReceiptItemsDto> GetReceiptItemById(int receiptItemId);
        Task CreateReceiptItem(CreateReceiptItemDto dto);
        Task UpdateReceiptItem(int receiptItemId, UpdateReceiptItemDto dto);
        Task DeleteReceiptItem(int receiptItemId);
    }
}
