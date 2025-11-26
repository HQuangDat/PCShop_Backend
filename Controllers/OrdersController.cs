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

        //=================Receipts Section
        [HttpGet("receipts")]
        public async Task<IActionResult> GetReceipts([FromQuery] GridifyQuery query)
        {
            var result = await _orderService.getReceipts(query);
            return Ok(result);
        }

        [HttpGet("admin/receipts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReceiptsByAdmin([FromQuery] GridifyQuery query)
        {
            var result = await _orderService.getAllReceiptsByAdmin(query);
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
        public async Task<IActionResult> GetReceiptItems(int receiptId, [FromQuery] GridifyQuery query)
        {
            var result = await _orderService.getReceiptItems(receiptId,query);
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

        //===================== Sales Statistics
        [HttpGet("sales-statistics")]
        public async Task<IActionResult> GetSalesStatistics([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            var result = await _orderService.GetSalesStatistics(startDate, endDate);
            Log.Information("Retrieved sales statistics from {StartDate} to {EndDate}", startDate, endDate);
            return Ok(result);
        }
    }
}
