using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Imperium.Core.Models;
using Imperium.Data.Repositories;

namespace Imperium.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            Products = new ProductRepository(_context);
            Dictionaries = new DictionaryRepository(_context);
            Orders = new OrderRepository(_context);
            Carts = new CartRepository(_context);
            Favorites = new GenericRepository<Favorite>(_context);
            Addresses = new GenericRepository<Address>(_context);
            Reviews = new GenericRepository<Review>(_context);
            Verifications = new GenericRepository<Verification>(_context);
            Files = new GenericRepository<File>(_context);
            ProductFiles = new GenericRepository<ProductFile>(_context);
            ProductColors = new GenericRepository<ProductColor>(_context);
            ProductSizes = new GenericRepository<ProductSize>(_context);
            OrderItems = new GenericRepository<OrderItem>(_context);
        }

        public IUserRepository Users { get; }
        public IProductRepository Products { get; }
        public IDictionaryRepository Dictionaries { get; }
        public IOrderRepository Orders { get; }
        public ICartRepository Carts { get; }
        public IGenericRepository<Favorite> Favorites { get; }
        public IGenericRepository<Address> Addresses { get; }
        public IGenericRepository<Review> Reviews { get; }
        public IGenericRepository<Verification> Verifications { get; }
        public IGenericRepository<File> Files { get; }
        public IGenericRepository<ProductFile> ProductFiles { get; }
        public IGenericRepository<ProductColor> ProductColors { get; }
        public IGenericRepository<ProductSize> ProductSizes { get; }
        public IGenericRepository<OrderItem> OrderItems { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}