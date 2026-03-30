using ShopAPI.Data;
using ShopAPI.Repoistires.Base;

namespace ShopAPI.Repoistires
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Dictionary<Type, object> _repositories = new();

        private readonly AppDbContext _context;
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }


        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }


        public IMainRepository<T> GetRepository<T>() where T : class
        {
            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
            {
                var repoInstance = new MainRepository<T>(_context);
                _repositories[type] = repoInstance;
            }

            return (IMainRepository<T>)_repositories[type];
        }
        public void Dispose()
        {
            _context.Dispose();
        }

    }
}
