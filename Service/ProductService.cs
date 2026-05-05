using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Dtos;
using PCShop_Backend.Dtos.ProductDtos.CreateDto;
using PCShop_Backend.Dtos.ProductDtos.UpdateDto;
using PCShop_Backend.Exceptions;
using PCShop_Backend.Interfaces;
using PCShop_Backend.Models;
using PCShop_Backend.Repositories.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PCShop_Backend.Service
{
    public class ProductService : IProductService
    {
        private readonly IComponentRepository _componentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheService _cacheService;

        public ProductService(IComponentRepository componentRepository, IHttpContextAccessor httpContextAccessor, ICacheService cacheService)
        {
            _componentRepository = componentRepository;
            _httpContextAccessor = httpContextAccessor;
            _cacheService = cacheService;
        }

        // ==================Component==================\\
        public async Task<Paging<ComponentDto>> getComponents(GridifyQuery query)
        {
            var rawKey = $"Components_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<ComponentDto>>(key);
            if (cachedData != null)
                return cachedData;

            var result = await _componentRepository.QueryComponents()
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

            if (result == null)
                throw new NotFoundException("No components found");

            await _cacheService.SetAsync(key, result);
            return result;
        }

        public async Task<ComponentDto> getComponentById(int id)
        {
            var rawKey = $"Component_{id}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<ComponentDto>(key);
            if (cachedData != null)
                return cachedData;

            var component = await _componentRepository.GetByIdAsync(id);
            if (component == null)
                throw new NotFoundException($"Component with ID {id} not found");

            var dto = new ComponentDto
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

            await _cacheService.SetAsync(key, dto);
            return dto;
        }

        public async Task createComponent(createComponentDto dto)
        {
            var component = new Models.Component
            {
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                Brand = dto.Brand,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                ImageUrl = dto.ImageUrl,
                IsActive = dto.IsActive ?? true,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };
            await _componentRepository.AddAsync(component);
            await _componentRepository.SaveChangesAsync();
        }

        public async Task updateComponent(int id, updateComponentDto dto)
        {
            int countTry = 0;
            int maxRetry = 3;
            while (countTry < maxRetry)
            {
                try
                {
                    var component = await _componentRepository.GetByIdAsync(id);
                    if (component == null)
                        throw new NotFoundException($"Component with ID {id} not found");

                    if (dto.Price <= 0)
                        throw new ValidationException("Price must be greater than 0");

                    if (dto.StockQuantity < 0)
                        throw new ValidationException("Stock quantity cannot be negative");

                    if (!await _componentRepository.CategoryExistsAsync(dto.CategoryId))
                        throw new ValidationException($"Category with ID {dto.CategoryId} not found");

                    component.Name = dto.Name;
                    component.CategoryId = dto.CategoryId;
                    component.Brand = dto.Brand;
                    component.Price = dto.Price;
                    component.StockQuantity = dto.StockQuantity;
                    component.ImageUrl = dto.ImageUrl;
                    component.IsActive = dto.IsActive;
                    component.Description = dto.Description;
                    component.UpdatedAt = DateTime.UtcNow;

                    await _componentRepository.SaveChangesAsync();

                    var rawKey = $"Component_{id}";
                    var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
                    await _cacheService.RemoveAsync(cacheKey);
                    break;
                }
                catch (DbUpdateConcurrencyException)
                {
                    countTry++;
                    if (countTry == maxRetry)
                        throw new Exception("The record was modified by another user. The edit operation was canceled.");
                }
            }
        }

        public async Task deleteComponent(int id)
        {
            var component = await _componentRepository.GetByIdAsync(id);
            if (component == null)
                throw new NotFoundException($"Component with ID {id} not found");

            if (await _componentRepository.IsUsedInPcBuildAsync(id) || await _componentRepository.IsUsedInActiveReceiptsAsync(id))
                throw new ConflictException($"Cannot delete component with ID {id} because it is in use.");

            component.IsActive = false;
            component.UpdatedAt = DateTime.UtcNow;
            await _componentRepository.SaveChangesAsync();

            var rawKey = $"Component_{id}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        // ==================ComponentCategory==================\\
        public async Task<Paging<ComponentCategoriesDto>> getComponentCategories(GridifyQuery query)
        {
            var rawKey = $"ComponentCategories_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<ComponentCategoriesDto>>(key);
            if (cachedData != null)
                return cachedData;

            var result = await _componentRepository.QueryCategories()
                .Select(cate => new ComponentCategoriesDto
                {
                    CategoryId = cate.CategoryId,
                    CategoryName = cate.CategoryName,
                    Description = cate.Description
                }).GridifyAsync(query);

            await _cacheService.SetAsync(key, result);
            return result;
        }

        public async Task<ComponentCategoriesDto?> getComponentCategoryById(int categoryId)
        {
            var rawKey = $"ComponentCategory_{categoryId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<ComponentCategoriesDto>(key);
            if (cachedData != null)
                return cachedData;

            var category = await _componentRepository.GetCategoryByIdAsync(categoryId);
            if (category == null)
                throw new NotFoundException($"Category with ID {categoryId} not found");

            var dto = new ComponentCategoriesDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description
            };

            await _cacheService.SetAsync(key, dto);
            return dto;
        }

        public async Task addComponentCategory(CreateComponentCategoryDto dto)
        {
            var category = new ComponentCategory
            {
                CategoryName = dto.CategoryName,
                Description = dto.Description
            };
            await _componentRepository.AddCategoryAsync(category);
            await _componentRepository.SaveChangesAsync();
        }

        public async Task updateComponentCategory(int categoryId, UpdateComponentCategoryDto dto)
        {
            var category = await _componentRepository.GetCategoryByIdAsync(categoryId);
            if (category == null)
                throw new NotFoundException($"Category with ID {categoryId} not found");

            category.CategoryName = dto.CategoryName;
            category.Description = dto.Description;
            await _componentRepository.SaveChangesAsync();

            var rawKey = $"ComponentCategory_{categoryId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        public async Task deleteComponentCategory(int categoryId)
        {
            var category = await _componentRepository.GetCategoryByIdAsync(categoryId);
            if (category == null)
                throw new NotFoundException($"Category with ID {categoryId} not found");

            _componentRepository.RemoveCategory(category);
            await _componentRepository.SaveChangesAsync();

            var rawKey = $"ComponentCategory_{categoryId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        // ==================ComponentSpec==================\\
        public async Task<Paging<ComponentSpecsDto>> getComponentSpecs(GridifyQuery query)
        {
            var rawKey = $"ComponentSpecs_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<ComponentSpecsDto>>(key);
            if (cachedData != null)
                return cachedData;

            var result = await _componentRepository.QuerySpecs()
                .Select(s => new ComponentSpecsDto
                {
                    SpecId = s.SpecId,
                    ComponentId = s.ComponentId,
                    SpecKey = s.SpecKey,
                    SpecValue = s.SpecValue,
                    DisplayOrder = s.DisplayOrder
                }).GridifyAsync(query);

            await _cacheService.SetAsync(key, result);
            return result;
        }

        public async Task<ComponentSpecsDto> getComponentSpecById(int specId)
        {
            var rawKey = $"ComponentSpec_{specId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<ComponentSpecsDto>(key);
            if (cachedData != null)
                return cachedData;

            var spec = await _componentRepository.GetSpecByIdAsync(specId);
            if (spec == null)
                throw new NotFoundException($"Component Spec with ID {specId} not found");

            var dto = new ComponentSpecsDto
            {
                SpecId = spec.SpecId,
                ComponentId = spec.ComponentId,
                SpecKey = spec.SpecKey,
                SpecValue = spec.SpecValue,
                DisplayOrder = spec.DisplayOrder
            };

            await _cacheService.SetAsync(key, dto);
            return dto;
        }

        public async Task addComponentSpecs(CreateComponentSpecDto dto)
        {
            await _componentRepository.AddSpecAsync(new ComponentSpec
            {
                ComponentId = dto.ComponentId,
                SpecKey = dto.SpecKey,
                SpecValue = dto.SpecValue,
                DisplayOrder = dto.DisplayOrder
            });
            await _componentRepository.SaveChangesAsync();
        }

        public async Task updateComponentSpecs(int specId, UpdateComponentSpecDto dto)
        {
            var spec = await _componentRepository.GetSpecByIdAsync(specId);
            if (spec == null)
                throw new NotFoundException($"Component Spec with ID {specId} not found");

            spec.SpecKey = dto.SpecKey;
            spec.SpecValue = dto.SpecValue;
            spec.DisplayOrder = dto.DisplayOrder;
            await _componentRepository.SaveChangesAsync();

            var rawKey = $"ComponentSpec_{specId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        public async Task deleteComponentSpecs(int specId)
        {
            var spec = await _componentRepository.GetSpecByIdAsync(specId);
            if (spec == null)
                throw new NotFoundException($"Component Spec with ID {specId} not found");

            _componentRepository.RemoveSpec(spec);
            await _componentRepository.SaveChangesAsync();

            var rawKey = $"ComponentSpec_{specId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        // ==================PC Build==================\\
        public async Task<Paging<PcBuildDto>> getPcBuilds(GridifyQuery query)
        {
            var rawKey = $"PcBuilds_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<PcBuildDto>>(key);
            if (cachedData != null)
                return cachedData;

            var result = await _componentRepository.QueryPcBuilds()
                .Select(b => new PcBuildDto
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
                }).GridifyAsync(query);

            await _cacheService.SetAsync(key, result);
            return result;
        }

        public async Task<PcBuildDto> getPcbuildById(int buildId)
        {
            var rawKey = $"PcBuild_{buildId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<PcBuildDto>(key);
            if (cachedData != null)
                return cachedData;

            var build = await _componentRepository.GetPcBuildByIdAsync(buildId);
            if (build == null)
                throw new NotFoundException($"Build {buildId} not found");

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

            var dto = new PcBuildDto
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
                TotalPrice = components.Sum(c => c.Subtotal)
            };

            await _cacheService.SetAsync(key, dto);
            return dto;
        }

        public async Task createPcbuild(CreatePcBuildDto dto)
        {
            var userIdClaims = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaims, out var userId);

            var componentIds = dto.Components.Select(c => c.ComponentId).ToList();
            var components = await _componentRepository.GetActiveComponentsByIdsAsync(componentIds);

            if (components.Count != componentIds.Distinct().Count())
                throw new ValidationException("One or more components are invalid or inactive");

            foreach (var item in dto.Components)
            {
                var component = components.First(c => c.ComponentId == item.ComponentId);
                if (component.StockQuantity < item.Quantity)
                    throw new ValidationException(
                        $"Component '{component.Name}' has insufficient stock. " +
                        $"Available: {component.StockQuantity}, Requested: {item.Quantity}");
            }

            var build = new Pcbuild
            {
                BuildName = dto.BuildName,
                Description = dto.Description,
                IsPublic = dto.IsPublic,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _componentRepository.AddPcBuildAsync(build);
            await _componentRepository.SaveChangesAsync();

            foreach (var item in dto.Components)
            {
                _componentRepository.AddPcBuildComponent(new PcbuildComponent
                {
                    BuildId = build.BuildId,
                    ComponentId = item.ComponentId,
                    Quantity = item.Quantity
                });
            }
            await _componentRepository.SaveChangesAsync();
        }

        public async Task UpdatePcBuild(int buildId, UpdatePcBuildDto dto)
        {
            var build = await _componentRepository.GetPcBuildByIdAsync(buildId);
            if (build == null)
                throw new NotFoundException($"PC Build with ID {buildId} not found");

            build.BuildName = dto.BuildName;
            build.Description = dto.Description;
            build.IsPublic = dto.IsPublic;
            build.UpdatedAt = DateTime.UtcNow;

            if (dto.Components != null && dto.Components.Any())
            {
                var componentIds = dto.Components.Select(c => c.ComponentId).Distinct().ToList();
                var validIds = await _componentRepository.GetActiveComponentIdsAsync(componentIds);

                if (validIds.Count != componentIds.Count)
                {
                    var invalidIds = componentIds.Except(validIds);
                    throw new ValidationException($"Components not found or inactive: {string.Join(", ", invalidIds)}");
                }

                var toRemove = build.PcbuildComponents
                    .Where(ec => !componentIds.Contains(ec.ComponentId))
                    .ToList();
                _componentRepository.RemovePcBuildComponents(toRemove);

                foreach (var item in dto.Components)
                {
                    var existing = build.PcbuildComponents
                        .FirstOrDefault(ec => ec.ComponentId == item.ComponentId);

                    if (existing != null)
                        existing.Quantity = item.Quantity;
                    else
                        _componentRepository.AddPcBuildComponent(new PcbuildComponent
                        {
                            BuildId = buildId,
                            ComponentId = item.ComponentId,
                            Quantity = item.Quantity
                        });
                }
            }

            await _componentRepository.SaveChangesAsync();

            var rawKey = $"PcBuild_{buildId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        public async Task deletePcbuild(int buildId)
        {
            var build = await _componentRepository.GetPcBuildByIdAsync(buildId);
            if (build == null)
                throw new NotFoundException($"PC Build with ID {buildId} not found");

            _componentRepository.RemovePcBuild(build);

            var rawKey = $"PcBuild_{buildId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);

            await _componentRepository.SaveChangesAsync();
        }
    }
}
