using Microsoft.AspNetCore.Mvc;
using PCShop_Backend.Dtos;
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

        // ==================Component==================\\
        [HttpGet("components")]
        public async Task<IActionResult> ComponentsList()
        {
            var components = await _productService.getAllComponents();
            return Ok(components);
        }

        [HttpGet("component/{id}")]
        public async Task<IActionResult> GetComponentById(int id)
        {
            var component = await _productService.getComponentById(id);
            if (component == null)
            {
                return NotFound(new { message = "Component not found" });
            }
            return Ok(component);
        }

        [HttpPost("component/create")]
        public async Task<IActionResult> CreateComponent([FromBody] createComponentDto createComponentDto)
        {
            await _productService.createComponent(createComponentDto);
            return Ok(new { message = "Component created successfully" });
        }

        [HttpPut("component/update/{id}")]
        public async Task<IActionResult> UpdateComponent(int id, [FromBody] updateComponentDto updateComponentDto)
        {
            await _productService.updateComponent(id, updateComponentDto);
            return Ok(new { message = "Component updated successfully" });
        }

        [HttpDelete("component/delete/{id}")]
        public async Task<IActionResult> DeleteComponent(int id)
        {
            await _productService.deleteComponent(id);
            return Ok();
        }

        // ==================ComponentCategory==================\\
        [HttpGet("component-categories")]
        public async Task<IActionResult> ComponentCategoriesList()
        {
            var categories = await _productService.getAllComponentCategories();
            return Ok(categories);
        }
        [HttpGet("component-category/{categoryId}")]
        public async Task<IActionResult> GetComponentCategoryById(int categoryId)
        {
            var category = await _productService.getComponentCategoryById(categoryId);
            if (category == null)
            {
                return NotFound(new { message = "Component category not found" });
            }
            return Ok(category);
        }

        [HttpPost("component-category/create")]
        public async Task<IActionResult> CreateComponentCategory([FromBody] CreateComponentCategoryDto createComponentCategoryDto)
        {
            await _productService.addComponentCategory(createComponentCategoryDto);
            return Ok(new { message = "Component category created successfully" });
        }

        [HttpPut("component-category/update/{categoryId}")]
        public async Task<IActionResult> UpdateComponentCategory(int categoryId, [FromBody] UpdateComponentCategoryDto updateComponentCategoryDto)
        {
            await _productService.updateComponentCategory(categoryId, updateComponentCategoryDto);
            return Ok(new { message = "Component category updated successfully" });
        }

        [HttpDelete("component-category/delete/{categoryId}")]
        public async Task<IActionResult> DeleteComponentCategory(int categoryId)
        {
            await _productService.deleteComponentCategory(categoryId);
            return Ok();
        }
    }
}
