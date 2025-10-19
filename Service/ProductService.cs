using Gridify;
using Gridify.EntityFramework;
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

        // ==================Component==================\\
        public async Task<Paging<ComponentDto>> getComponents(GridifyQuery query)
        {
            var componentsQuery = _context.Components
                .Include(ct => ct.Category)
                .Include(sp => sp.ComponentSpecs)
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
                });
            var result = await componentsQuery.GridifyAsync(query);
            return result;
        }
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
        public async Task<ComponentDto> getComponentById(int id)
        {
            var component = await _context.Components
                .Include(c => c.Category)
                .Include(c => c.ComponentSpecs)
                .FirstOrDefaultAsync(c => c.ComponentId == id);

            if (component == null)
            {
                throw new NotFoundException($"Component with ID {id} not found");
            }

            return new ComponentDto
            {
                ComponentId = component.ComponentId,
                Name = component.Name,
                CategoryName = component.Category != null ? component.Category.CategoryName : "Uncategorized",
                Brand = component.Brand!,
                Price = component.Price,
                StockQuantity = component.StockQuantity,
                Description = component.Description!,
                ImageUrl = component.ImageUrl!,
                Specs = component.ComponentSpecs.Select(s => new ComponentSpecDto
                {
                    SpecKey = s.SpecKey,
                    SpecValue = s.SpecValue,
                    DisplayOrder = s.DisplayOrder
                }).ToList()
            };
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

        // ==================ComponentCategory==================\\
        public async Task<Paging<ComponentCategoriesDto>> getComponentCategories(GridifyQuery query)
        {
            var categoriesQuery = _context.ComponentCategories
                .Select(cate => new ComponentCategoriesDto
                {
                    CategoryId = cate.CategoryId,
                    CategoryName = cate.CategoryName,
                    Description = cate.Description
                });
            var result = await categoriesQuery.GridifyAsync(query);
            return result;
        }
        public async Task addComponentCategory(CreateComponentCategoryDto createComponentCategoryDto)
        {
            var category = new ComponentCategory
            {
                CategoryName = createComponentCategoryDto.CategoryName,
                Description = createComponentCategoryDto.Description
            };
            await _context.ComponentCategories.AddAsync(category);
        }
        public async Task<ComponentCategoriesDto?> getComponentCategoryById(int categoryId)
        {
            var category = await _context.ComponentCategories.FindAsync(categoryId);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {categoryId} not found");
            }
            return new ComponentCategoriesDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description
            };
        }
        public Task updateComponentCategory(int componentId, UpdateComponentCategoryDto updateComponentCategoryDto)
        {
            var category = _context.ComponentCategories.Find(componentId);
            if (category == null)
            {
                throw new NotFoundException($"Component with ID {componentId} not found");
            }
            category.CategoryName = updateComponentCategoryDto.CategoryName;
            category.Description = updateComponentCategoryDto.Description;
            return _context.SaveChangesAsync();
        }
        public async Task deleteComponentCategory(int categoryId)
        {
            var category = await _context.ComponentCategories.FindAsync(categoryId);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {categoryId} not found");
            }
            _context.ComponentCategories.Remove(category);
            await _context.SaveChangesAsync();
        }

        // ==================ComponentSpec==================\\
        public async Task addComponentSpecs(CreateComponentSpecDto createComponentSpecDto)
        {
            await _context.ComponentSpecs.AddAsync(new ComponentSpec
            {
                ComponentId = createComponentSpecDto.ComponentId,
                SpecKey = createComponentSpecDto.SpecKey,
                SpecValue = createComponentSpecDto.SpecValue,
                DisplayOrder = createComponentSpecDto.DisplayOrder
            });
            await _context.SaveChangesAsync();
        }
        public async Task<Paging<ComponentSpecsDto>> getComponentSpecs(GridifyQuery query)
        {
            var specsQuery = _context.ComponentSpecs
                .Select(s => new ComponentSpecsDto
                {
                    SpecId = s.SpecId,
                    ComponentId = s.ComponentId,
                    SpecKey = s.SpecKey,
                    SpecValue = s.SpecValue,
                    DisplayOrder = s.DisplayOrder
                });
            var result = await specsQuery.GridifyAsync(query);
            return result;
        }
        public async Task<ComponentSpecsDto> getComponentSpecById(int specId)
        {
            var spec = await _context.ComponentSpecs.FindAsync(specId);
            if(spec == null)
            {
                throw new NotFoundException($"Component Spec with ID {specId} not found");
            }

            return new ComponentSpecsDto
            {
                SpecId = spec.SpecId,
                ComponentId = spec.ComponentId,
                SpecKey = spec.SpecKey,
                SpecValue = spec.SpecValue,
                DisplayOrder = spec.DisplayOrder
            };
        }
        public async Task updateComponentSpecs(int specId, UpdateComponentSpecDto updateComponentSpecDto)
        {
            var spec = await _context.ComponentSpecs.FindAsync(specId);
            if (spec == null)
            {
                throw new NotFoundException($"Component Spec with ID {specId} not found");
            }
            spec.SpecKey = updateComponentSpecDto.SpecKey;
            spec.SpecValue = updateComponentSpecDto.SpecValue;
            spec.DisplayOrder = updateComponentSpecDto.DisplayOrder;
            await _context.SaveChangesAsync();
        }
        public async Task deleteComponentSpecs(int specId)
        {
            var spec = _context.ComponentSpecs.Find(specId);
            if (spec == null)
            {
                throw new NotFoundException($"Component Spec with ID {specId} not found");
            }
            _context.ComponentSpecs.Remove(spec);
            await _context.SaveChangesAsync();
        }

        // ==================PC Build==================\\
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