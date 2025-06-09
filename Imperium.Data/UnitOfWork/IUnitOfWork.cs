using Imperium.Data.Repositories;

namespace Imperium.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IProductRepository Products { get; }
        IDictionaryRepository Dictionaries { get; }
        IOrderRepository Orders { get; }
        ICartRepository Carts { get; }
        IGenericRepository<Favorite> Favorites { get; }
        IGenericRepository<Address> Addresses { get; }
        IGenericRepository<Review> Reviews { get; }
        IGenericRepository<Verification> Verifications { get; }
        IGenericRepository<File> Files { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}