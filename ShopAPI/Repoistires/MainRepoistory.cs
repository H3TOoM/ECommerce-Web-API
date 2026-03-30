using Microsoft.EntityFrameworkCore;
using ShopAPI.Data;
using ShopAPI.Helpers;
using ShopAPI.Repoistires.Base;

namespace ShopAPI.Repoistires
{
    public class MainRepository<T> : IMainRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public MainRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<T> CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existingEntity = await GetByIdAsync(id);
            if (existingEntity is null)
            {
                return false;
            }

            _dbSet.Remove(existingEntity);
            return true;
        }

        public async Task<T?> UpdateAsync(int id, T entity)
        {
            var existingEntity = await GetByIdAsync(id);
            if (existingEntity is null)
            {
                return null;
            }

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }
    }
}
