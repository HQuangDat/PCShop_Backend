using Humanizer;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using PCShop_Backend.Data;
using PCShop_Backend.Dtos;
using PCShop_Backend.Models;
using System.ComponentModel.DataAnnotations;

namespace PCShop_Backend.Service
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Component
        public async Task createComponent(createComponentDto createComponentDto)
        {
            var component = new Component
            {
                Name = createComponentDto.Name,
                CategoryId = createComponentDto.CategoryId,
                Brand = createComponentDto.Brand,
                Price = createComponentDto.Price,
                StockQuantity = createComponentDto.StockQuantity,
                ImageUrl = createComponentDto.ImageUrl,
                IsActive = createComponentDto.IsActive ?? true,
                Description = createComponentDto.Description,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Components.AddAsync(component);
            await  _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<ComponentDto>> getAllComponents()
        {
            return await _context.Components
                .Include(ct=>ct.Category)
                .Include(sp=>sp.ComponentSpecs)
                .Select(c => new ComponentDto
                {
                    ComponentId = c.ComponentId,
                    Name = c.Name,
                    CategoryName = c.Category != null ? c.Category.CategoryName : "Uncategorized",
                    Brand = c.Brand!,
                    Price = c.Price,
                    StockQuantity = c.StockQuantity,
                    Description = c.Description!,
                    ImageUrl = c.ImageUrl!,
                    Specs = c.ComponentSpecs.Select(s => new ComponentSpecDto
                    {
                        SpecKey = s.SpecKey,
                        SpecValue = s.SpecValue,
                        DisplayOrder = s.DisplayOrder
                    }).ToList()
                })
                .ToListAsync();
        }
        public async Task<Component?> getComponentById(int id)
        {
            return await _context.Components.FindAsync(id);
        }
        public async Task<IEnumerable<Component>> getComponentsByCategory(int CategoryId)
        {
            return await _context.Components
                .Where(c=>c.CategoryId == CategoryId)
                .ToListAsync();
        }
        public async Task updateComponent(int id, updateComponentDto updateComponentDto)
        {
            var component = await _context.Components.FindAsync(id);

            if (component == null)
            {
                throw new NotFoundException($"Component with ID {id} not found");
            }

            if (updateComponentDto.Price <= 0)
            {
                throw new ValidationException("Price must be greater than 0");
            }

            if (updateComponentDto.StockQuantity < 0)
            {
                throw new ValidationException("Stock quantity cannot be negative");
            }

            var categoryExists = await _context.ComponentCategories
                .AnyAsync(c => c.CategoryId == updateComponentDto.CategoryId);

            if (!categoryExists)
            {
                throw new ValidationException($"Category with ID {updateComponentDto.CategoryId} not found");
            }

            component.Name = updateComponentDto.Name;
            component.CategoryId = updateComponentDto.CategoryId;
            component.Brand = updateComponentDto.Brand;
            component.Price = updateComponentDto.Price;
            component.StockQuantity = updateComponentDto.StockQuantity;
            component.ImageUrl = updateComponentDto.ImageUrl;
            component.IsActive = updateComponentDto.IsActive;
            component.Description = updateComponentDto.Description;
            component.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        public async Task deleteComponent(int id)
        {
            var component =  await _context.Components.FindAsync(id);
            if (component == null)
                throw new NotFoundException($"Component with ID {id} not found");
            
            var isInUsePcBuild = await _context.PcbuildComponents
                .AnyAsync(pc => pc.ComponentId == id);

            var isUsedInActiveReceipts = await _context.ReceiptItems
                .Include(ri => ri.Receipt)
                .AnyAsync(ri => ri.ComponentId == id &&
                               ri.Receipt.Status != "Cancelled" &&
                               ri.Receipt.Status != "Delivered");

            if(isInUsePcBuild || isUsedInActiveReceipts)
            {
                throw new Exception($"Cannot delete component with ID {id} because it is in use.");
            }

            component.IsActive = false;
            component.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ComponentCategory
        public Task addComponentCategory(ComponentCategory newCategory)
        {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<ComponentCategory>> getAllComponentCategories()
        {
            throw new NotImplementedException();
        }
        public Task<ComponentCategory?> getComponentCategoryById(int categoryId)
        {
            throw new NotImplementedException();
        }
        public Task updateComponentCategory(int componentId, ComponentCategory componentCategory)
        {
            throw new NotImplementedException();
        }
        public Task deleteComponentCategory(int categoryId)
        {
            throw new NotImplementedException();
        }

        // ComponentSpec
        public Task addComponentSpecs(int componentId, ComponentSpec spec)
        {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<ComponentSpec>> getAllComponentSpecs()
        {
            throw new NotImplementedException();
        }
        public Task<ComponentSpec?> getComponentSpecsById(int specId)
        {
            throw new NotImplementedException();
        }
        public Task updateComponentSpecs(int specId, ComponentSpec updatedSpec)
        {
            throw new NotImplementedException();
        }
        public Task deleteComponentSpecs(int specId)
        {
            throw new NotImplementedException();
        }

        // Pcbuild
        public Task createPcbuild(Pcbuild pcBuild)
        {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<Pcbuild>> getAllPcbuilds()
        {
            throw new NotImplementedException();
        }
        public Task<Pcbuild?> getPcbuildById(int buildId)
        {
            throw new NotImplementedException();
        }
        public Task updatePcbuild(int buildId, Pcbuild updatedBuild)
        {
            throw new NotImplementedException();
        }
        public Task deletePcbuild(int buildId)
        {
            throw new NotImplementedException();
        }
    }
}