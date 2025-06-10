using System;
using System.Threading.Tasks;
using Imperium.Data.Repositories;
using Imperium.Core.Models;

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
        IGenericRepository<ProductFile> ProductFiles { get; }
        IGenericRepository<ProductColor> ProductColors { get; }
        IGenericRepository<ProductSize> ProductSizes { get; }
        IGenericRepository<OrderItem> OrderItems { get; }
        IGenericRepository<Log> Logs { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}