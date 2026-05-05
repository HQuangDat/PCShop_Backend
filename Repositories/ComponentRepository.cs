using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Data;
using PCShop_Backend.Models;
using PCShop_Backend.Repositories.Interfaces;

namespace PCShop_Backend.Repositories
{
    public class ComponentRepository : IComponentRepository
    {
        private readonly ApplicationDbContext _context;

        public ComponentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Components
        public IQueryable<Component> QueryComponents()
        {
            return _context.Components
                .Include(c => c.Category)
                .Include(c => c.ComponentSpecs);
        }

        public async Task<Component?> GetByIdAsync(int id)
        {
            return await _context.Components
                .Include(c => c.Category)
                .Include(c => c.ComponentSpecs)
                .FirstOrDefaultAsync(c => c.ComponentId == id);
        }

        public async Task AddAsync(Component component)
        {
            await _context.Components.AddAsync(component);
        }

        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.ComponentCategories.AnyAsync(c => c.CategoryId == categoryId);
        }

        public async Task<bool> IsUsedInPcBuildAsync(int componentId)
        {
            return await _context.PcbuildComponents.AnyAsync(pc => pc.ComponentId == componentId);
        }

        public async Task<bool> IsUsedInActiveReceiptsAsync(int componentId)
        {
            return await _context.ReceiptItems
                .Include(ri => ri.Receipt)
                .AnyAsync(ri => ri.ComponentId == componentId &&
                               ri.Receipt.Status != "Cancelled" &&
                               ri.Receipt.Status != "Delivered");
        }

        // Categories
        public IQueryable<ComponentCategory> QueryCategories()
        {
            return _context.ComponentCategories;
        }

        public async Task<ComponentCategory?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.ComponentCategories.FindAsync(categoryId);
        }

        public async Task AddCategoryAsync(ComponentCategory category)
        {
            await _context.ComponentCategories.AddAsync(category);
        }

        public void RemoveCategory(ComponentCategory category)
        {
            _context.ComponentCategories.Remove(category);
        }

        // Specs
        public IQueryable<ComponentSpec> QuerySpecs()
        {
            return _context.ComponentSpecs;
        }

        public async Task<ComponentSpec?> GetSpecByIdAsync(int specId)
        {
            return await _context.ComponentSpecs.FindAsync(specId);
        }

        public async Task AddSpecAsync(ComponentSpec spec)
        {
            await _context.ComponentSpecs.AddAsync(spec);
        }

        public void RemoveSpec(ComponentSpec spec)
        {
            _context.ComponentSpecs.Remove(spec);
        }

        // PcBuilds
        public IQueryable<Pcbuild> QueryPcBuilds()
        {
            return _context.Pcbuilds
                .Include(b => b.CreatedByUser)
                .Include(b => b.PcbuildComponents)
                    .ThenInclude(bc => bc.Component)
                        .ThenInclude(c => c.Category);
        }

        public async Task<Pcbuild?> GetPcBuildByIdAsync(int buildId)
        {
            return await _context.Pcbuilds
                .Include(b => b.CreatedByUser)
                .Include(b => b.PcbuildComponents)
                    .ThenInclude(bc => bc.Component)
                        .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync(b => b.BuildId == buildId);
        }

        public async Task AddPcBuildAsync(Pcbuild build)
        {
            await _context.Pcbuilds.AddAsync(build);
        }

        public void RemovePcBuild(Pcbuild build)
        {
            _context.Pcbuilds.Remove(build);
        }

        public async Task<List<Component>> GetActiveComponentsByIdsAsync(List<int> ids)
        {
            return await _context.Components
                .Where(c => ids.Contains(c.ComponentId) && c.IsActive == true)
                .ToListAsync();
        }

        public async Task<List<int>> GetActiveComponentIdsAsync(List<int> ids)
        {
            return await _context.Components
                .Where(c => ids.Contains(c.ComponentId) && c.IsActive == true)
                .Select(c => c.ComponentId)
                .ToListAsync();
        }

        public void AddPcBuildComponent(PcbuildComponent component)
        {
            _context.PcbuildComponents.Add(component);
        }

        public void RemovePcBuildComponents(IEnumerable<PcbuildComponent> components)
        {
            _context.PcbuildComponents.RemoveRange(components);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
