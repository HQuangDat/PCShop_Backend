using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PCShop_Backend.Dtos.CartDtos.CreateDtos;
using PCShop_Backend.Dtos.CartDtos.UpdateDtos;
using PCShop_Backend.Service;

namespace PCShop_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }


        //============Cart Items
        [HttpGet("cart-items")]
        public async Task<IActionResult> GetCartItems([FromQuery] GridifyQuery query)
        {
            var result = await _cartService.getCartItems(query);
            return Ok(result);
        }

        [HttpPost("cart-items")]
        public async Task<IActionResult> AddToCart([FromBody] AddItemToCartDtos dto)
        {
            await _cartService.AddToCart(dto);
            return Ok(new { message = "Added item to cart." });
        }

        [HttpPut("cart-items/{cartId}")]
        public async Task<IActionResult> UpdateCartItems(int cartId, [FromBody] UpdateCartItemsDto dto)
        {
            await _cartService.UpdateCartItems(cartId, dto);
            return Ok(new { message = "Updated cart item." });
        }

        [HttpDelete("cart-items/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            await _cartService.RemoveFromCart(cartItemId);
            return Ok(new { message = "Removed item from cart." });
        }

        [HttpDelete("cart-items/clear")]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearCart();
            return Ok(new { message = "Cleared all items from cart." });
        }
    }
}
