using Microsoft.AspNetCore.Mvc;
using PCShop_Backend.Service;

namespace PCShop_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("components")]
        public async Task<IActionResult> ComponentsList()
        {
            var components = await _productService.getAllComponents();
            return Ok(components);
        }
    }
}
