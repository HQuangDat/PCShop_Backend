using PCShop_Backend.Models;
using System.Runtime.CompilerServices;

namespace PCShop_Backend.Service
{
    public interface IProductService
    {
        //Component 
        Task<IEnumerable<Component>> getAllComponents();
        Task<Component?> getComponentById(int id);
        Task<IEnumerable<Component>> getComponentsByCategory(int CategoryId);
        Task createComponent(Component newComponent);
        Task updateComponent(int id, Component updatedComponent);
        Task deleteComponent(int id);
        //Component Specs
        Task addComponentSpecs(int componentId, ComponentSpec spec);
        Task<ComponentSpec?> getComponentSpecsById(int specId);
        Task updateComponentSpecs(int specId, ComponentSpec updatedSpec);
        Task deleteComponentSpecs(int specId);
        Task<IEnumerable<ComponentSpec>> getAllComponentSpecs();
        //Component Category
        Task<ComponentCategory?> getComponentCategoryById(int categoryId);
        Task addComponentCategory(ComponentCategory newCategory);
        Task deleteComponentCategory(int categoryId);
        Task updateComponentCategory(int categoryId, ComponentCategory componentCategory);
        Task<IEnumerable<ComponentCategory>> getAllComponentCategories();
        //Pcbuild
        Task<Pcbuild?> getPcbuildById(int buildId);
        Task createPcbuild(Pcbuild pcBuild);
        Task updatePcbuild(int buildId, Pcbuild updatedBuild);
        Task deletePcbuild(int buildId);
        Task<IEnumerable<Pcbuild>> getAllPcbuilds();
    }
}
