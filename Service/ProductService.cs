using Gridify;
using Gridify.EntityFramework;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using OpenQA.Selenium;
using PCShop_Backend.Data;
using PCShop_Backend.Dtos;
using PCShop_Backend.Dtos.ProductDtos.CreateDto;
using PCShop_Backend.Dtos.ProductDtos.UpdateDto;
using PCShop_Backend.Exceptions;
using PCShop_Backend.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PCShop_Backend.Service
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _distributedCache;

        public ProductService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IDistributedCache distributedCache)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _distributedCache = distributedCache;
        }

        // ==================Component==================\\
        public async Task<Paging<ComponentDto>> getComponents(GridifyQuery query)
        {
            var key = $"Components_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}".GetHashCode().ToString();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };
            //check if data is cached
            var cachedData = await _distributedCache.GetStringAsync(key);
            if(!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<Paging<ComponentDto>>(cachedData)!;
            }
            //if not cached, query the database
            var componentsQuery = await _context.Components
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
                }).GridifyAsync(query);

            if (componentsQuery == null)
            {
                throw new Exceptions.NotFoundException("No components found");
            }

            //cached that data
            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(componentsQuery), options);

            return componentsQuery;
        }
        public async Task createComponent(createComponentDto createComponentDto)
        {
            var component = new Models.Component
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
            var key = $"Component_{id}".GetHashCode().ToString();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            var cachedData = await _distributedCache.GetStringAsync(key);
            if(!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<ComponentDto>(cachedData)!;
            }

            var component = await _context.Components
                .Include(c => c.Category)
                .Include(c => c.ComponentSpecs)
                .FirstOrDefaultAsync(c => c.ComponentId == id);

            if (component == null)
            {
                throw new Exceptions.NotFoundException($"Component with ID {id} not found");
            }

            var componentDto = new ComponentDto
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

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(componentDto), options);

            return componentDto;
        }
        public async Task updateComponent(int id, updateComponentDto updateComponentDto)
        {
            int countTry = 0;
            int maxRetry = 3;
            while (countTry < maxRetry)
            {
                try
                {
                    var component = await _context.Components.FindAsync(id);

                    if (component == null)
                    {
                        throw new Exceptions.NotFoundException($"Component with ID {id} not found");
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
                    var key = $"Component_{id}".GetHashCode().ToString();
                    await _distributedCache.RemoveAsync(key);
                    break;
                }
                catch (DbUpdateConcurrencyException)
                {
                    countTry++;
                    if (countTry == maxRetry)
                    {
                        throw new Exception("The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled.");
                    }
                }
            }
        }
        public async Task deleteComponent(int id)
        {
            var component =  await _context.Components.FindAsync(id);
            if (component == null)
                throw new Exceptions.NotFoundException($"Component with ID {id} not found");
            
            var isInUsePcBuild = await _context.PcbuildComponents
                .AnyAsync(pc => pc.ComponentId == id);

            var isUsedInActiveReceipts = await _context.ReceiptItems
                .Include(ri => ri.Receipt)
                .AnyAsync(ri => ri.ComponentId == id &&
                               ri.Receipt.Status != "Cancelled" &&
                               ri.Receipt.Status != "Delivered");

            if(isInUsePcBuild || isUsedInActiveReceipts)
            {
                throw new ConflictException($"Cannot delete component with ID {id} because it is in use.");
            }

            component.IsActive = false;
            component.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var key = $"Component_{id}".GetHashCode().ToString();
            await _distributedCache.RemoveAsync(key);
        }

        // ==================ComponentCategory==================\\
        public async Task<Paging<ComponentCategoriesDto>> getComponentCategories(GridifyQuery query)
        {
            var key = $"ComponentCategories_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}".GetHashCode().ToString();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            var cachedData = await _distributedCache.GetStringAsync(key);
            if(!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<Paging<ComponentCategoriesDto>>(cachedData)!;
            }

            var categoriesQuery = _context.ComponentCategories
                .Select(cate => new ComponentCategoriesDto
                {
                    CategoryId = cate.CategoryId,
                    CategoryName = cate.CategoryName,
                    Description = cate.Description
                });
            var result = await categoriesQuery.GridifyAsync(query);

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(result), options);

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
            var key = $"ComponentCategory_{categoryId}".GetHashCode().ToString();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            var cachedData = await _distributedCache.GetStringAsync(key);
            if(!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<ComponentCategoriesDto>(cachedData)!;
            }

            var category = await _context.ComponentCategories.FindAsync(categoryId);
            if (category == null)
            {
                throw new Exceptions.NotFoundException($"Category with ID {categoryId} not found");
            }

            var categoryDto = new ComponentCategoriesDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description
            };

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(categoryDto), options);

            return categoryDto;
        }
        public async Task updateComponentCategory(int componentId, UpdateComponentCategoryDto updateComponentCategoryDto)
        {
            var category = _context.ComponentCategories.Find(componentId);
            if (category == null)
            {
                throw new Exceptions.NotFoundException($"Component with ID {componentId} not found");
            }
            category.CategoryName = updateComponentCategoryDto.CategoryName;
            category.Description = updateComponentCategoryDto.Description;

            var key = $"ComponentCategory_{componentId}".GetHashCode().ToString();
            await _distributedCache.RemoveAsync(key);

            await _context.SaveChangesAsync();
        }
        public async Task deleteComponentCategory(int categoryId)
        {
            var category = await _context.ComponentCategories.FindAsync(categoryId);
            if (category == null)
            {
                throw new Exceptions.NotFoundException($"Category with ID {categoryId} not found");
            }
            _context.ComponentCategories.Remove(category);
            await _context.SaveChangesAsync();

            var key = $"ComponentCategory_{categoryId}".GetHashCode().ToString();
            await _distributedCache.RemoveAsync(key);
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
            var key = $"ComponentSpecs_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}".GetHashCode().ToString();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            var cachedData = await _distributedCache.GetStringAsync(key);
            if(!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<Paging<ComponentSpecsDto>>(cachedData)!;
            }

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

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(result), options);

            return result;
        }
        public async Task<ComponentSpecsDto> getComponentSpecById(int specId)
        {
            var key = $"ComponentSpec_{specId}".GetHashCode().ToString();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            var cachedData = await _distributedCache.GetStringAsync(key);
            if(!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<ComponentSpecsDto>(cachedData)!;
            }

            var spec = await _context.ComponentSpecs.FindAsync(specId);
            if(spec == null)
            {
                throw new Exceptions.NotFoundException($"Component Spec with ID {specId} not found");
            }

            var specDto = new ComponentSpecsDto
            {
                SpecId = spec.SpecId,
                ComponentId = spec.ComponentId,
                SpecKey = spec.SpecKey,
                SpecValue = spec.SpecValue,
                DisplayOrder = spec.DisplayOrder
            };

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(specDto), options);

            return specDto;
        }
        public async Task updateComponentSpecs(int specId, UpdateComponentSpecDto updateComponentSpecDto)
        {
            var spec = await _context.ComponentSpecs.FindAsync(specId);
            if (spec == null)
            {
                throw new Exceptions.NotFoundException($"Component Spec with ID {specId} not found");
            }
            spec.SpecKey = updateComponentSpecDto.SpecKey;
            spec.SpecValue = updateComponentSpecDto.SpecValue;
            spec.DisplayOrder = updateComponentSpecDto.DisplayOrder;
            await _context.SaveChangesAsync();

            var key = $"ComponentSpec_{specId}".GetHashCode().ToString();
            await _distributedCache.RemoveAsync(key);
        }
        public async Task deleteComponentSpecs(int specId)
        {
            var spec = _context.ComponentSpecs.Find(specId);
            if (spec == null)
            {
                throw new Exceptions.NotFoundException($"Component Spec with ID {specId} not found");
            }
            _context.ComponentSpecs.Remove(spec);
            await _context.SaveChangesAsync();

            var key = $"ComponentSpec_{specId}".GetHashCode().ToString();
            await _distributedCache.RemoveAsync(key);
        }

        // ==================PC Build==================\\

        public async Task<Paging<PcBuildDto>> getPcBuilds(GridifyQuery query)
        {
            var key = $"PcBuilds_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}".GetHashCode().ToString();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            var cachedData = await _distributedCache.GetStringAsync(key);
            if(!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<Paging<PcBuildDto>>(cachedData)!;
            }

            var build =  _context.Pcbuilds
                .Include(b => b.CreatedByUser)
                .Include(b => b.PcbuildComponents)
                    .ThenInclude(bc => bc.Component)
                        .ThenInclude(c => c.Category)
                .Select(b=> new PcBuildDto
                {
                    BuildId = b.BuildId,
                    BuildName = b.BuildName,
                    Description = b.Description,
                    IsPublic = b.IsPublic ?? false,
                    CreatedByUserId = b.CreatedByUserId,
                    CreatedByUserName = b.CreatedByUser!.FullName ?? "Unknown",
                    CreatedAt = b.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = b.UpdatedAt,
                    Components = b.PcbuildComponents.Select(bc => new PcBuildComponentDto
                    {
                        ComponentId = bc.ComponentId,
                        ComponentName = bc.Component.Name,
                        CategoryName = bc.Component.Category.CategoryName ?? "N/A",
                        Brand = bc.Component.Brand,
                        UnitPrice = bc.Component.Price,
                        Quantity = bc.Quantity,
                        Subtotal = bc.Component.Price * bc.Quantity,
                        ImageUrl = bc.Component.ImageUrl
                    }).ToList(),
                    TotalPrice = b.PcbuildComponents.Sum(bc => bc.Component.Price * bc.Quantity)
                });
            var result = await build.GridifyAsync(query);

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(result), options);

            return result;
        }
        public async Task<PcBuildDto> getPcbuildById(int buildId)
        {
            var key = $"PcBuild_{buildId}".GetHashCode().ToString();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            var cachedData = await _distributedCache.GetStringAsync(key);
            if(!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<PcBuildDto>(cachedData)!;
            }

            var build = await _context.Pcbuilds
                .Include(b => b.CreatedByUser)
                .Include(b => b.PcbuildComponents)
                    .ThenInclude(bc => bc.Component)
                        .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync(b => b.BuildId == buildId);

            if (build == null)
                throw new Exceptions.NotFoundException($"Build {buildId} not found");

            var components = build.PcbuildComponents.Select(bc => new PcBuildComponentDto
            {
                ComponentId = bc.ComponentId,
                ComponentName = bc.Component.Name,
                CategoryName = bc.Component.Category?.CategoryName ?? "N/A",
                Brand = bc.Component.Brand,
                UnitPrice = bc.Component.Price,
                Quantity = bc.Quantity,
                Subtotal = bc.Component.Price * bc.Quantity,
                ImageUrl = bc.Component.ImageUrl
            }).ToList();

            var buildDto = new PcBuildDto
            {
                BuildId = build.BuildId,
                BuildName = build.BuildName,
                Description = build.Description,
                IsPublic = build.IsPublic ?? false,
                CreatedByUserId = build.CreatedByUserId,
                CreatedByUserName = build.CreatedByUser?.FullName ?? "Unknown",
                CreatedAt = build.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = build.UpdatedAt,
                Components = components,
                TotalPrice = components.Sum(c => c.Subtotal),
            };

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(buildDto), options);

            return buildDto;
        }
        public async Task createPcbuild(CreatePcBuildDto createPcBuildDto)
        {
            var userIdClaims = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaims, out var userId);

            var componentsId = createPcBuildDto.Components.Select(c => c.ComponentId).ToList();
            var components = await _context.Components.Where(c => componentsId
                .Contains(c.ComponentId) && c.IsActive == true)
                .ToListAsync();
            if(components.Count != componentsId.Distinct().Count())
            {
                throw new ValidationException("One or more components are invalid or inactive");
            }

            //check quantity of each component
            foreach(var item in createPcBuildDto.Components)
            {
                var componentInBuild = components.First(c => c.ComponentId == item.ComponentId);
                if(componentInBuild.StockQuantity < item.Quantity)
                {
                    throw new ValidationException(
                            $"Component '{componentInBuild.Name}' has insufficient stock. " +
                            $"Available: {componentInBuild.StockQuantity}, Requested: {item.Quantity}");
                }
            }

            var createPc = new Pcbuild
            {
                BuildName = createPcBuildDto.BuildName,
                Description = createPcBuildDto.Description,
                IsPublic = createPcBuildDto.IsPublic,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Pcbuilds.AddAsync(createPc);
            await _context.SaveChangesAsync();

            foreach (var item in createPcBuildDto.Components)
            {
                _context.PcbuildComponents.Add(new PcbuildComponent
                {
                    BuildId = createPc.BuildId,
                    ComponentId = item.ComponentId,
                    Quantity = item.Quantity
                });
            }
            await _context.SaveChangesAsync();
        }
        public async Task UpdatePcBuild(int buildId, UpdatePcBuildDto dto)
        {
            var userIdClaims = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaims, out var userId);

            var build = await _context.Pcbuilds
                .Include(b => b.PcbuildComponents)  
                .FirstOrDefaultAsync(b => b.BuildId == buildId);

            if (build == null)
                throw new Exceptions.NotFoundException($"PC Build with ID {buildId} not found");

            build.BuildName = dto.BuildName;
            build.Description = dto.Description;
            build.IsPublic = dto.IsPublic;
            build.UpdatedAt = DateTime.UtcNow;

            if (dto.Components != null && dto.Components.Any())
            {
                var componentIds = dto.Components.Select(c => c.ComponentId).Distinct().ToList();
                var validComponents = await _context.Components
                    .Where(c => componentIds.Contains(c.ComponentId) && c.IsActive == true)
                    .Select(c => c.ComponentId)
                    .ToListAsync();

                if (validComponents.Count != componentIds.Count)
                {
                    var invalidIds = componentIds.Except(validComponents);
                    throw new ValidationException(
                        $"Components not found or inactive: {string.Join(", ", invalidIds)}");
                }

                var existingComponents = build.PcbuildComponents.ToList();

                var componentsToRemove = existingComponents
                    .Where(ec => !componentIds.Contains(ec.ComponentId))
                    .ToList();

                _context.PcbuildComponents.RemoveRange(componentsToRemove);

                foreach (var item in dto.Components)
                {
                    var existing = existingComponents
                        .FirstOrDefault(ec => ec.ComponentId == item.ComponentId);

                    if (existing != null)
                    {
                        // Update quantity
                        existing.Quantity = item.Quantity;
                    }
                    else
                    {
                        _context.PcbuildComponents.Add(new PcbuildComponent
                        {
                            BuildId = buildId,
                            ComponentId = item.ComponentId,
                            Quantity = item.Quantity
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            var key = $"PcBuild_{buildId}".GetHashCode().ToString();
            await _distributedCache.RemoveAsync(key);
        }
        public Task deletePcbuild(int buildId)
        {
            var pcBuild = _context.Pcbuilds.Find(buildId);
            if (pcBuild == null)
            {
                throw new Exceptions.NotFoundException($"PC Build with ID {buildId} not found");
            }
            _context.Pcbuilds.Remove(pcBuild);

            var key = $"PcBuild_{buildId}".GetHashCode().ToString();
            _distributedCache.RemoveAsync(key);

            return _context.SaveChangesAsync();
        }

        
    }
}