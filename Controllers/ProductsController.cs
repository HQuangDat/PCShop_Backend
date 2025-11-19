using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCShop_Backend.Dtos.ProductDtos.CreateDto;
using PCShop_Backend.Dtos.ProductDtos.UpdateDto;
using PCShop_Backend.Service;
using PCShop_Backend.Exceptions;
using Serilog;

namespace PCShop_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // ==================Component==================\\
        [AllowAnonymous]
        [HttpGet("components")]
        public async Task<IActionResult> ComponentsList([FromQuery] GridifyQuery query)
        {
            var components = await _productService.getComponents(query);
            Log.Information("Fetched components list");
            return Ok(components);
        }

        [AllowAnonymous]
        [HttpGet("component/{id}")]
        public async Task<IActionResult> GetComponentById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Component ID must be greater than 0.");

            var component = await _productService.getComponentById(id);
            if (component == null)
                throw new NotFoundException($"Component with ID {id} not found.");

            Log.Information("Fetched component with ID {ComponentId}", id);
            return Ok(component);
        }

        [HttpPost("component/create")]
        public async Task<IActionResult> CreateComponent([FromBody] createComponentDto createComponentDto)
        {
            await _productService.createComponent(createComponentDto);
            Log.Information("Component created successfully: {ComponentName}", createComponentDto.Name);
            return Ok(new { message = "Component created successfully" });
        }

        [HttpPut("component/update/{id}")]
        public async Task<IActionResult> UpdateComponent(int id, [FromBody] updateComponentDto updateComponentDto)
        {
            if (id <= 0)
                throw new ArgumentException("Component ID must be greater than 0.");

            await _productService.updateComponent(id, updateComponentDto);
            Log.Information("Component updated successfully with ID {ComponentId}", id);
            return Ok(new { message = "Component updated successfully" });
        }

        [HttpDelete("component/delete/{id}")]
        public async Task<IActionResult> DeleteComponent(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Component ID must be greater than 0.");

            await _productService.deleteComponent(id);
            Log.Information("Component deleted successfully with ID {ComponentId}", id);
            return Ok(new { message = "Component deleted successfully" });
        }

        // ==================ComponentCategory==================\\
        [AllowAnonymous]
        [HttpGet("component-categories")]
        public async Task<IActionResult> ComponentCategoriesList([FromQuery] GridifyQuery query)
        {
            var categories = await _productService.getComponentCategories(query);
            Log.Information("Fetched component categories list");
            return Ok(categories);
        }

        [AllowAnonymous]
        [HttpGet("component-category/{categoryId}")]
        public async Task<IActionResult> GetComponentCategoryById(int categoryId)
        {
            if (categoryId <= 0)
                throw new ArgumentException("Category ID must be greater than 0.");

            var category = await _productService.getComponentCategoryById(categoryId);
            if (category == null)
                throw new NotFoundException($"Component category with ID {categoryId} not found.");

            Log.Information("Fetched component category with ID {CategoryId}", categoryId);
            return Ok(category);
        }

        [HttpPost("component-category/create")]
        public async Task<IActionResult> CreateComponentCategory([FromBody] CreateComponentCategoryDto createComponentCategoryDto)
        {
            await _productService.addComponentCategory(createComponentCategoryDto);
            Log.Information("Component category created successfully: {CategoryName}", createComponentCategoryDto.CategoryName);
            return Ok(new { message = "Component category created successfully" });
        }

        [HttpPut("component-category/update/{categoryId}")]
        public async Task<IActionResult> UpdateComponentCategory(int categoryId, [FromBody] UpdateComponentCategoryDto updateComponentCategoryDto)
        {
            if (categoryId <= 0)
                throw new ArgumentException("Category ID must be greater than 0.");

            await _productService.updateComponentCategory(categoryId, updateComponentCategoryDto);
            Log.Information("Component category updated successfully with ID {CategoryId}", categoryId);
            return Ok(new { message = "Component category updated successfully" });
        }

        [HttpDelete("component-category/delete/{categoryId}")]
        public async Task<IActionResult> DeleteComponentCategory(int categoryId)
        {
            if (categoryId <= 0)
                throw new ArgumentException("Category ID must be greater than 0.");

            await _productService.deleteComponentCategory(categoryId);
            Log.Information("Component category deleted successfully with ID {CategoryId}", categoryId);
            return Ok(new { message = "Component category deleted successfully" });
        }

        // ==================ComponentSpecs==================\\
        [AllowAnonymous]
        [HttpGet("component-specs")]
        public async Task<IActionResult> ComponentSpecsList([FromQuery] GridifyQuery query)
        {
            var specs = await _productService.getComponentSpecs(query);
            Log.Information("Fetched component specs list");
            return Ok(specs);
        }

        [AllowAnonymous]
        [HttpGet("component-spec/{specId}")]
        public async Task<IActionResult> GetComponentSpecById(int specId)
        {
            if (specId <= 0)
                throw new ArgumentException("Spec ID must be greater than 0.");

            var spec = await _productService.getComponentSpecById(specId);
            if (spec == null)
                throw new NotFoundException($"Component spec with ID {specId} not found.");

            Log.Information("Fetched component spec with ID {SpecId}", specId);
            return Ok(spec);
        }
        
        [HttpPost("component-spec/create")]
        public async Task<IActionResult> CreateComponentSpec([FromBody] CreateComponentSpecDto createComponentSpecDto)
        {
            await _productService.addComponentSpecs(createComponentSpecDto);
            Log.Information("Component spec created successfully: {SpecKey}", createComponentSpecDto.SpecKey);
            return Ok(new { message = "Component spec created successfully" });
        }

        [HttpPut("component-spec/update/{specId}")]
        public async Task<IActionResult> UpdateComponentSpec(int specId, [FromBody] UpdateComponentSpecDto updateComponentSpecDto)
        {
            if (specId <= 0)
                throw new ArgumentException("Spec ID must be greater than 0.");

            await _productService.updateComponentSpecs(specId, updateComponentSpecDto);
            Log.Information("Component spec updated successfully with ID {SpecId}", specId);
            return Ok(new { message = "Component spec updated successfully" });
        }

        [HttpDelete("component-spec/delete/{specId}")]
        public async Task<IActionResult> DeleteComponentSpec(int specId)
        {
            if (specId <= 0)
                throw new ArgumentException("Spec ID must be greater than 0.");

            await _productService.deleteComponentSpecs(specId);
            Log.Information("Component spec deleted successfully with ID {SpecId}", specId);
            return Ok(new { message = "Component spec deleted successfully" });
        }

        // ==================PCBuild==================\\
        [AllowAnonymous]
        [HttpGet("pcbuilds")]
        public async Task<IActionResult> PcBuildsList([FromQuery] GridifyQuery query)
        {
            var pcBuilds = await _productService.getPcBuilds(query);
            Log.Information("Fetched PC builds list");
            return Ok(pcBuilds);
        }

        [AllowAnonymous]
        [HttpGet("pcbuild/{id}")]
        public async Task<IActionResult> GetPcBuildById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("PC Build ID must be greater than 0.");

            var pcBuild = await _productService.getPcbuildById(id);
            if (pcBuild == null)
                throw new NotFoundException($"PC Build with ID {id} not found.");

            Log.Information("Fetched PC build with ID {PCBuildId}", id);
            return Ok(pcBuild);
        }

        [HttpPost("pcbuild/create")]
        public async Task<IActionResult> CreatePcBuild([FromBody] CreatePcBuildDto createPcBuildDto)
        {
            await _productService.createPcbuild(createPcBuildDto);
            Log.Information("PC Build created successfully: {BuildName}", createPcBuildDto.BuildName);
            return Ok(new { message = "PC Build created successfully" });
        }

        [HttpPut("pcbuild/update/{id}")]
        public async Task<IActionResult> UpdatePcBuild(int id, [FromBody] UpdatePcBuildDto updatePcBuildDto)
        {
            if (id <= 0)
                throw new ArgumentException("PC Build ID must be greater than 0.");

            await _productService.UpdatePcBuild(id, updatePcBuildDto);
            Log.Information("PC Build updated successfully with ID {PCBuildId}", id);
            return Ok(new { message = "PC Build updated successfully" });
        }

        [HttpDelete("pcbuild/delete/{id}")]
        public async Task<IActionResult> DeletePcBuild(int id)
        {
            if (id <= 0)
                throw new ArgumentException("PC Build ID must be greater than 0.");

            await _productService.deletePcbuild(id);
            Log.Information("PC Build deleted successfully with ID {PCBuildId}", id);
            return Ok(new { message = "PC Build deleted successfully" });
        }
    }
}
