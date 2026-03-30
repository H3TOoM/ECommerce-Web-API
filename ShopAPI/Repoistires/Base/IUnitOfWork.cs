namespace ShopAPI.Repoistires.Base
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync();

        IMainRepository<T> GetRepository<T>() where T : class;
    }
}
