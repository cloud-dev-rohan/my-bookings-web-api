using MyBookingsWebApi.Data;
using MyBookingsWebApi.Models;

namespace MyBookingsWebApi.Repository
{
    public interface IInventoryRepository
    {
        Task<Inventory?> GetByIdAsync(Guid id);
        Task AddAsync(Inventory inventory);
        Task UpdateAsync(Inventory inventory);
    }
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;
        public InventoryRepository(AppDbContext context) => _context = context;

        public async Task<Inventory?> GetByIdAsync(Guid id)
        {
            return await _context.Inventories.FindAsync(id);
        }

        public async Task AddAsync(Inventory inventory)
        {
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Inventory inventory)
        {
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
        }
    }
}
