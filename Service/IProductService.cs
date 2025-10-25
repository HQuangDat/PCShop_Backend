using Gridify;
using PCShop_Backend.Dtos;
using PCShop_Backend.Dtos.ProductDtos.CreateDto;
using PCShop_Backend.Dtos.ProductDtos.UpdateDto;
using PCShop_Backend.Models;

namespace PCShop_Backend.Service
{
    public interface IProductService
    {
        //Component 
        Task<Paging<ComponentDto>> getComponents(GridifyQuery model);
        Task<ComponentDto> getComponentById(int id);
        Task createComponent(createComponentDto createComponentDto);
        Task updateComponent(int id, updateComponentDto updateComponentDto);
        Task deleteComponent(int id);
        //Component Specs
        Task<Paging<ComponentSpecsDto>> getComponentSpecs(GridifyQuery query);
        Task addComponentSpecs(CreateComponentSpecDto createComponentSpecDto);
        Task<ComponentSpecsDto> getComponentSpecById(int specId);
        Task updateComponentSpecs(int specId, UpdateComponentSpecDto updateComponentSpecDto);
        Task deleteComponentSpecs(int specId);
        //Component Category
        Task<Paging<ComponentCategoriesDto>> getComponentCategories(GridifyQuery query);
        Task<ComponentCategoriesDto?> getComponentCategoryById(int categoryId);
        Task addComponentCategory(CreateComponentCategoryDto createComponentCategoryDto);
        Task deleteComponentCategory(int categoryId);
        Task updateComponentCategory(int categoryId, UpdateComponentCategoryDto updateComponentCategoryDto);
        //Pcbuild
        Task<Paging<PcBuildDto>> getPcBuilds(GridifyQuery query);
        Task<PcBuildDto> getPcbuildById(int buildId);
        Task createPcbuild(CreatePcBuildDto createPcBuildDto);
        Task UpdatePcBuild(int buildId, UpdatePcBuildDto dto);
        Task deletePcbuild(int buildId);
    }
}
