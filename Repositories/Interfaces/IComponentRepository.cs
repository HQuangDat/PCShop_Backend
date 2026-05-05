using PCShop_Backend.Models;

namespace PCShop_Backend.Repositories.Interfaces
{
    public interface IComponentRepository
    {
        // Components
        IQueryable<Component> QueryComponents();
        Task<Component?> GetByIdAsync(int id);
        Task AddAsync(Component component);
        Task<bool> CategoryExistsAsync(int categoryId);
        Task<bool> IsUsedInPcBuildAsync(int componentId);
        Task<bool> IsUsedInActiveReceiptsAsync(int componentId);

        // Categories
        IQueryable<ComponentCategory> QueryCategories();
        Task<ComponentCategory?> GetCategoryByIdAsync(int categoryId);
        Task AddCategoryAsync(ComponentCategory category);
        void RemoveCategory(ComponentCategory category);

        // Specs
        IQueryable<ComponentSpec> QuerySpecs();
        Task<ComponentSpec?> GetSpecByIdAsync(int specId);
        Task AddSpecAsync(ComponentSpec spec);
        void RemoveSpec(ComponentSpec spec);

        // PcBuilds
        IQueryable<Pcbuild> QueryPcBuilds();
        Task<Pcbuild?> GetPcBuildByIdAsync(int buildId);
        Task AddPcBuildAsync(Pcbuild build);
        void RemovePcBuild(Pcbuild build);
        Task<List<Component>> GetActiveComponentsByIdsAsync(List<int> ids);
        Task<List<int>> GetActiveComponentIdsAsync(List<int> ids);
        void AddPcBuildComponent(PcbuildComponent component);
        void RemovePcBuildComponents(IEnumerable<PcbuildComponent> components);

        Task SaveChangesAsync();
    }
}
