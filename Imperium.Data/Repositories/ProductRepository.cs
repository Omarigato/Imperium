using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Imperium.Core.Models;

namespace Imperium.Data.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Material)
                .Where(p => p.CategoryId == categoryId && p.IsAvailable)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetFeaturedAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Material)
                .Where(p => p.IsFeatured && p.IsAvailable)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAvailableAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Material)
                .Where(p => p.IsAvailable)
                .ToListAsync();
        }

        public async Task<Product?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Material)
                .FirstOrDefaultAsync(p => p.Code == code);
        }

        public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Material)
                .Where(p => p.IsAvailable && 
                           (p.NameRu.Contains(searchTerm) || 
                            p.NameKz.Contains(searchTerm) ||
                            p.DescriptionRu!.Contains(searchTerm) ||
                            p.DescriptionKz!.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<Product?> GetWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Material)
                .Include(p => p.ProductColors).ThenInclude(pc => pc.Color)
                .Include(p => p.ProductSizes).ThenInclude(ps => ps.Size)
                .Include(p => p.ProductFiles).ThenInclude(pf => pf.File)
                .Include(p => p.Reviews).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}