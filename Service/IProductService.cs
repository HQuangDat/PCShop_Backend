using PCShop_Backend.Dtos;
using PCShop_Backend.Models;

namespace PCShop_Backend.Service
{
    public interface IProductService
    {
        //Component 
        Task<IEnumerable<ComponentDto>> getAllComponents();
        Task<ComponentDto> getComponentById(int id);
        Task<IEnumerable<ComponentDto>> getComponentsByCategory(int CategoryId);
        Task createComponent(createComponentDto createComponentDto);
        Task updateComponent(int id, updateComponentDto updateComponentDto);
        Task deleteComponent(int id);
        //Component Specs
        Task addComponentSpecs(int componentId, ComponentSpec spec);
        Task<ComponentSpec?> getComponentSpecsById(int specId);
        Task updateComponentSpecs(int specId, ComponentSpec updatedSpec);
        Task deleteComponentSpecs(int specId);
        Task<IEnumerable<ComponentSpec>> getAllComponentSpecs();
        //Component Category
        Task<ComponentCategoriesDto?> getComponentCategoryById(int categoryId);
        Task addComponentCategory(CreateComponentCategoryDto createComponentCategoryDto);
        Task deleteComponentCategory(int categoryId);
        Task updateComponentCategory(int categoryId, UpdateComponentCategoryDto updateComponentCategoryDto);
        Task<IEnumerable<ComponentCategoriesDto>> getAllComponentCategories();
        //Pcbuild
        Task<Pcbuild?> getPcbuildById(int buildId);
        Task createPcbuild(Pcbuild pcBuild);
        Task updatePcbuild(int buildId, Pcbuild updatedBuild);
        Task deletePcbuild(int buildId);
        Task<IEnumerable<Pcbuild>> getAllPcbuilds();
    }
}
