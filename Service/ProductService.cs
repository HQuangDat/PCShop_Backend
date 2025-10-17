using PCShop_Backend.Data;
using PCShop_Backend.Models;

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
        public Task createComponent(Component newComponent)
        {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<Component>> getAllComponents()
        {
            throw new NotImplementedException();
        }
        public Task<Component?> getComponentById(int id)
        {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<Component>> getComponentsByCategory(int CategoryId)
        {
            throw new NotImplementedException();
        }
        public Task updateComponent(int id, Component updatedComponent)
        {
            throw new NotImplementedException();
        }
        public Task deleteComponent(int id)
        {
            throw new NotImplementedException();
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
        public Task updateComponentCategory(int categoryId, ComponentCategory componentCategory)
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