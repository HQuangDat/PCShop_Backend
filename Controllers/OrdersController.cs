using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCShop_Backend.Dtos.OrderDtos.CreateDtos;
using PCShop_Backend.Dtos.OrderDtos.UpdateDtos;
using PCShop_Backend.Service;
using Serilog;

namespace PCShop_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        //============Cart Items
        [HttpGet("cart-items")]
        public async Task<IActionResult> GetCartItems([FromQuery] GridifyQuery query)
        {
            var result = await _orderService.getCartItems(query);
            return Ok(result);
        }

        [HttpPost("cart-items")]
        public async Task<IActionResult> AddToCart([FromBody] AddItemToCartDtos dto)
        {
            await _orderService.AddToCart(dto);
            return Ok(new { message = "Added item to cart." });
        }

        [HttpPut("cart-items/{cartId}")]
        public async Task<IActionResult> UpdateCartItems(int cartId, [FromBody] UpdateCartItemsDto dto)
        {
            await _orderService.UpdateCartItems(cartId, dto);
            return Ok(new { message = "Updated cart item." });
        }

        [HttpDelete("cart-items/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            await _orderService.RemoveFromCart(cartItemId);
            return Ok(new { message = "Removed item from cart." });
        }

        [HttpDelete("cart-items/clear")]
        public async Task<IActionResult> ClearCart()
        {
            await _orderService.ClearCart();
            return Ok(new { message = "Cleared all items from cart." });
        }

        //=================Receipts Section
        [HttpGet("receipts")]
        public async Task<IActionResult> GetReceipts([FromQuery] GridifyQuery query)
        {
            var result = await _orderService.getReceipts(query);
            return Ok(result);
        }

        [HttpGet("receipts/{receiptId}")]
        public async Task<IActionResult> GetReceiptById(int receiptId)
        {
            var result = await _orderService.getReceiptById(receiptId);
            return Ok(result);
        }

        [HttpPost("receipts")]
        public async Task<IActionResult> CreateReceipt([FromBody] CreateReceiptDto dto)
        {
            await _orderService.CreateReceipt(dto);
            return Ok(new { message = "Created receipt." });
        }

        [HttpPut("receipts/{receiptId}")]
        public async Task<IActionResult> UpdateReceipt(int receiptId, [FromBody] UpdateReceiptDto dto)
        {
            await _orderService.UpdateReceipt(receiptId, dto);
            return Ok(new { message = "Updated receipt." });
        }

        [HttpDelete("receipts/{receiptId}")]
        public async Task<IActionResult> DeleteReceipt(int receiptId)
        {
            await _orderService.DeleteReceipt(receiptId);
            return Ok(new { message = "Deleted receipt." });
        }

        //===================== Receipt Items Section
        [HttpGet("receipt-items")]
        public async Task<IActionResult> GetReceiptItems([FromQuery] GridifyQuery query)
        {
            var result = await _orderService.getReceiptItems(query);
            return Ok(result);
        }

        [HttpGet("receipt-items/{receiptItemId}")]
        public async Task<IActionResult> GetReceiptItemById(int receiptItemId)
        {
            var result = await _orderService.GetReceiptItemById(receiptItemId);
            return Ok(result);
        }

        [HttpPost("receipt-items")]
        public async Task<IActionResult> CreateReceiptItem([FromBody] CreateReceiptItemDto dto)
        {
            await _orderService.CreateReceiptItem(dto);
            return Ok(new { message = "Created receipt item." });
        }

        [HttpPut("receipt-items/{receiptItemId}")]
        public async Task<IActionResult> UpdateReceiptItem(int receiptItemId, [FromBody] UpdateReceiptItemDto dto)
        {
            await _orderService.UpdateReceiptItem(receiptItemId, dto);
            return Ok(new { message = "Updated receipt item." });
        }

        [HttpDelete("receipt-items/{receiptItemId}")]
        public async Task<IActionResult> DeleteReceiptItem(int receiptItemId)
        {
            await _orderService.DeleteReceiptItem(receiptItemId);
            return Ok(new { message = "Deleted receipt item." });
        }
    }
}
