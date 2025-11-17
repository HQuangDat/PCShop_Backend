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
            try
            {
                var components = await _productService.getComponents(query);
                Log.Information("Fetched components list");
                return Ok(components);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching components list");
                throw;
            }
        }

        [AllowAnonymous]
        [HttpGet("component/{id}")]
        public async Task<IActionResult> GetComponentById(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Component ID must be greater than 0.");

                var component = await _productService.getComponentById(id);
                if (component == null)
                    throw new NotFoundException($"Component with ID {id} not found.");

                Log.Information("Fetched component with ID {ComponentId}", id);
                return Ok(component);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching component with ID {ComponentId}", id);
                throw;
            }
        }

        [HttpPost("component/create")]
        public async Task<IActionResult> CreateComponent([FromBody] createComponentDto createComponentDto)
        {
            try
            {
                if (createComponentDto == null)
                    throw new ArgumentException("Component data cannot be null.");

                if (string.IsNullOrWhiteSpace(createComponentDto.Name))
                    throw new ArgumentException("Component name is required.");

                await _productService.createComponent(createComponentDto);
                Log.Information("Component created successfully: {ComponentName}", createComponentDto.Name);
                return Ok(new { message = "Component created successfully" });
            }
            catch (ConflictException ex)
            {
                Log.Warning("Conflict while creating component: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating component");
                throw;
            }
        }

        [HttpPut("component/update/{id}")]
        public async Task<IActionResult> UpdateComponent(int id, [FromBody] updateComponentDto updateComponentDto)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Component ID must be greater than 0.");

                if (updateComponentDto == null)
                    throw new ArgumentException("Component data cannot be null.");

                if (string.IsNullOrWhiteSpace(updateComponentDto.Name))
                    throw new ArgumentException("Component name is required.");

                await _productService.updateComponent(id, updateComponentDto);
                Log.Information("Component updated successfully with ID {ComponentId}", id);
                return Ok(new { message = "Component updated successfully" });
            }
            catch (NotFoundException ex)
            {
                Log.Warning("Component not found while updating: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating component with ID {ComponentId}", id);
                throw;
            }
        }

        [HttpDelete("component/delete/{id}")]
        public async Task<IActionResult> DeleteComponent(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Component ID must be greater than 0.");

                await _productService.deleteComponent(id);
                Log.Information("Component deleted successfully with ID {ComponentId}", id);
                return Ok(new { message = "Component deleted successfully" });
            }
            catch (NotFoundException ex)
            {
                Log.Warning("Component not found while deleting: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting component with ID {ComponentId}", id);
                throw;
            }
        }

        // ==================ComponentCategory==================\\
        [AllowAnonymous]
        [HttpGet("component-categories")]
        public async Task<IActionResult> ComponentCategoriesList([FromQuery] GridifyQuery query)
        {
            try
            {
                var categories = await _productService.getComponentCategories(query);
                Log.Information("Fetched component categories list");
                return Ok(categories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching component categories list");
                throw;
            }
        }

        [AllowAnonymous]
        [HttpGet("component-category/{categoryId}")]
        public async Task<IActionResult> GetComponentCategoryById(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                    throw new ArgumentException("Category ID must be greater than 0.");

                var category = await _productService.getComponentCategoryById(categoryId);
                if (category == null)
                    throw new NotFoundException($"Component category with ID {categoryId} not found.");

                Log.Information("Fetched component category with ID {CategoryId}", categoryId);
                return Ok(category);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching component category with ID {CategoryId}", categoryId);
                throw;
            }
        }

        [HttpPost("component-category/create")]
        public async Task<IActionResult> CreateComponentCategory([FromBody] CreateComponentCategoryDto createComponentCategoryDto)
        {
            try
            {
                if (createComponentCategoryDto == null)
                    throw new ArgumentException("Component category data cannot be null.");

                if (string.IsNullOrWhiteSpace(createComponentCategoryDto.CategoryName))
                    throw new ArgumentException("Category name is required.");

                await _productService.addComponentCategory(createComponentCategoryDto);
                Log.Information("Component category created successfully: {CategoryName}", createComponentCategoryDto.CategoryName);
                return Ok(new { message = "Component category created successfully" });
            }
            catch (ConflictException ex)
            {
                Log.Warning("Conflict while creating component category: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating component category");
                throw;
            }
        }

        [HttpPut("component-category/update/{categoryId}")]
        public async Task<IActionResult> UpdateComponentCategory(int categoryId, [FromBody] UpdateComponentCategoryDto updateComponentCategoryDto)
        {
            try
            {
                if (categoryId <= 0)
                    throw new ArgumentException("Category ID must be greater than 0.");

                if (updateComponentCategoryDto == null)
                    throw new ArgumentException("Component category data cannot be null.");

                if (string.IsNullOrWhiteSpace(updateComponentCategoryDto.CategoryName))
                    throw new ArgumentException("Category name is required.");

                await _productService.updateComponentCategory(categoryId, updateComponentCategoryDto);
                Log.Information("Component category updated successfully with ID {CategoryId}", categoryId);
                return Ok(new { message = "Component category updated successfully" });
            }
            catch (NotFoundException ex)
            {
                Log.Warning("Component category not found while updating: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating component category with ID {CategoryId}", categoryId);
                throw;
            }
        }

        [HttpDelete("component-category/delete/{categoryId}")]
        public async Task<IActionResult> DeleteComponentCategory(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                    throw new ArgumentException("Category ID must be greater than 0.");

                await _productService.deleteComponentCategory(categoryId);
                Log.Information("Component category deleted successfully with ID {CategoryId}", categoryId);
                return Ok(new { message = "Component category deleted successfully" });
            }
            catch (NotFoundException ex)
            {
                Log.Warning("Component category not found while deleting: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting component category with ID {CategoryId}", categoryId);
                throw;
            }
        }

        // ==================ComponentSpecs==================\\
        [AllowAnonymous]
        [HttpGet("component-specs")]
        public async Task<IActionResult> ComponentSpecsList([FromQuery] GridifyQuery query)
        {
            try
            {
                var specs = await _productService.getComponentSpecs(query);
                Log.Information("Fetched component specs list");
                return Ok(specs);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching component specs list");
                throw;
            }
        }

        [AllowAnonymous]
        [HttpGet("component-spec/{specId}")]
        public async Task<IActionResult> GetComponentSpecById(int specId)
        {
            try
            {
                if (specId <= 0)
                    throw new ArgumentException("Spec ID must be greater than 0.");

                var spec = await _productService.getComponentSpecById(specId);
                if (spec == null)
                    throw new NotFoundException($"Component spec with ID {specId} not found.");

                Log.Information("Fetched component spec with ID {SpecId}", specId);
                return Ok(spec);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching component spec with ID {SpecId}", specId);
                throw;
            }
        }
        
        [HttpPost("component-spec/create")]
        public async Task<IActionResult> CreateComponentSpec([FromBody] CreateComponentSpecDto createComponentSpecDto)
        {
            try
            {
                if (createComponentSpecDto == null)
                    throw new ArgumentException("Component spec data cannot be null.");

                if (string.IsNullOrWhiteSpace(createComponentSpecDto.SpecKey))
                    throw new ArgumentException("Spec key is required.");

                if (string.IsNullOrWhiteSpace(createComponentSpecDto.SpecValue))
                    throw new ArgumentException("Spec value is required.");

                await _productService.addComponentSpecs(createComponentSpecDto);
                Log.Information("Component spec created successfully: {SpecKey}", createComponentSpecDto.SpecKey);
                return Ok(new { message = "Component spec created successfully" });
            }
            catch (ConflictException ex)
            {
                Log.Warning("Conflict while creating component spec: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating component spec");
                throw;
            }
        }

        [HttpPut("component-spec/update/{specId}")]
        public async Task<IActionResult> UpdateComponentSpec(int specId, [FromBody] UpdateComponentSpecDto updateComponentSpecDto)
        {
            try
            {
                if (specId <= 0)
                    throw new ArgumentException("Spec ID must be greater than 0.");

                if (updateComponentSpecDto == null)
                    throw new ArgumentException("Component spec data cannot be null.");

                if (string.IsNullOrWhiteSpace(updateComponentSpecDto.SpecKey))
                    throw new ArgumentException("Spec key is required.");

                if (string.IsNullOrWhiteSpace(updateComponentSpecDto.SpecValue))
                    throw new ArgumentException("Spec value is required.");

                await _productService.updateComponentSpecs(specId, updateComponentSpecDto);
                Log.Information("Component spec updated successfully with ID {SpecId}", specId);
                return Ok(new { message = "Component spec updated successfully" });
            }
            catch (NotFoundException ex)
            {
                Log.Warning("Component spec not found while updating: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating component spec with ID {SpecId}", specId);
                throw;
            }
        }

        [HttpDelete("component-spec/delete/{specId}")]
        public async Task<IActionResult> DeleteComponentSpec(int specId)
        {
            try
            {
                if (specId <= 0)
                    throw new ArgumentException("Spec ID must be greater than 0.");

                await _productService.deleteComponentSpecs(specId);
                Log.Information("Component spec deleted successfully with ID {SpecId}", specId);
                return Ok(new { message = "Component spec deleted successfully" });
            }
            catch (NotFoundException ex)
            {
                Log.Warning("Component spec not found while deleting: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting component spec with ID {SpecId}", specId);
                throw;
            }
        }

        // ==================PCBuild==================\\
        [AllowAnonymous]
        [HttpGet("pcbuilds")]
        public async Task<IActionResult> PcBuildsList([FromQuery] GridifyQuery query)
        {
            try
            {
                var pcBuilds = await _productService.getPcBuilds(query);
                Log.Information("Fetched PC builds list");
                return Ok(pcBuilds);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching PC builds list");
                throw;
            }
        }

        [AllowAnonymous]
        [HttpGet("pcbuild/{id}")]
        public async Task<IActionResult> GetPcBuildById(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("PC Build ID must be greater than 0.");

                var pcBuild = await _productService.getPcbuildById(id);
                if (pcBuild == null)
                    throw new NotFoundException($"PC Build with ID {id} not found.");

                Log.Information("Fetched PC build with ID {PCBuildId}", id);
                return Ok(pcBuild);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching PC build with ID {PCBuildId}", id);
                throw;
            }
        }

        [HttpPost("pcbuild/create")]
        public async Task<IActionResult> CreatePcBuild([FromBody] CreatePcBuildDto createPcBuildDto)
        {
            try
            {
                if (createPcBuildDto == null)
                    throw new ArgumentException("PC Build data cannot be null.");

                if (string.IsNullOrWhiteSpace(createPcBuildDto.BuildName))
                    throw new ArgumentException("Build name is required.");

                await _productService.createPcbuild(createPcBuildDto);
                Log.Information("PC Build created successfully: {BuildName}", createPcBuildDto.BuildName);
                return Ok(new { message = "PC Build created successfully" });
            }
            catch (ConflictException ex)
            {
                Log.Warning("Conflict while creating PC Build: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating PC Build");
                throw;
            }
        }

        [HttpPut("pcbuild/update/{id}")]
        public async Task<IActionResult> UpdatePcBuild(int id, [FromBody] UpdatePcBuildDto updatePcBuildDto)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("PC Build ID must be greater than 0.");

                if (updatePcBuildDto == null)
                    throw new ArgumentException("PC Build data cannot be null.");

                if (string.IsNullOrWhiteSpace(updatePcBuildDto.BuildName))
                    throw new ArgumentException("Build name is required.");

                await _productService.UpdatePcBuild(id, updatePcBuildDto);
                Log.Information("PC Build updated successfully with ID {PCBuildId}", id);
                return Ok(new { message = "PC Build updated successfully" });
            }
            catch (NotFoundException ex)
            {
                Log.Warning("PC Build not found while updating: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating PC Build with ID {PCBuildId}", id);
                throw;
            }
        }

        [HttpDelete("pcbuild/delete/{id}")]
        public async Task<IActionResult> DeletePcBuild(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("PC Build ID must be greater than 0.");

                await _productService.deletePcbuild(id);
                Log.Information("PC Build deleted successfully with ID {PCBuildId}", id);
                return Ok(new { message = "PC Build deleted successfully" });
            }
            catch (NotFoundException ex)
            {
                Log.Warning("PC Build not found while deleting: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting PC Build with ID {PCBuildId}", id);
                throw;
            }
        }
    }
}
