using Microsoft.EntityFrameworkCore;
using Imperium.Core.Models;
using Imperium.Core.Enums;

namespace Imperium.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Dictionary> Dictionaries { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductFile> ProductFiles { get; set; }
        public DbSet<ProductColor> ProductColors { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Verification> Verifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Phone).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasConversion<string>();
                entity.HasMany(e => e.Favorites).WithOne(f => f.User).HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Carts).WithOne(c => c.User).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Addresses).WithOne(a => a.User).HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Orders).WithOne(o => o.User).HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.Reviews).WithOne(r => r.User).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.Verifications).WithOne(v => v.User).HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            // Dictionary (self-referencing)
            modelBuilder.Entity<Dictionary>(entity =>
            {
                entity.ToTable("dictionaries");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasOne(e => e.Parent)
                      .WithMany(e => e.Children)
                      .HasForeignKey(e => e.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(d => d.ProductsAsCategory).WithOne(p => p.Category).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(d => d.ProductsAsMaterial).WithOne(p => p.Material).HasForeignKey(p => p.MaterialId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(d => d.ProductColors).WithOne(pc => pc.Color).HasForeignKey(pc => pc.ColorId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(d => d.ProductSizes).WithOne(ps => ps.Size).HasForeignKey(ps => ps.SizeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(d => d.CartsAsColor).WithOne(c => c.SelectedColor).HasForeignKey(c => c.SelectedColorId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(d => d.CartsAsSize).WithOne(c => c.SelectedSize).HasForeignKey(c => c.SelectedSizeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(d => d.OrderItemsAsColor).WithOne(oi => oi.SelectedColor).HasForeignKey(oi => oi.SelectedColorId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(d => d.OrderItemsAsSize).WithOne(oi => oi.SelectedSize).HasForeignKey(oi => oi.SelectedSizeId).OnDelete(DeleteBehavior.Restrict);
            });

            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasOne(e => e.Category)
                      .WithMany(d => d.ProductsAsCategory)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Material)
                      .WithMany(d => d.ProductsAsMaterial)
                      .HasForeignKey(e => e.MaterialId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.ProductColors).WithOne(pc => pc.Product).HasForeignKey(pc => pc.ProductId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.ProductSizes).WithOne(ps => ps.Product).HasForeignKey(ps => ps.ProductId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Favorites).WithOne(f => f.Product).HasForeignKey(f => f.ProductId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Carts).WithOne(c => c.Product).HasForeignKey(c => c.ProductId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.OrderItems).WithOne(oi => oi.Product).HasForeignKey(oi => oi.ProductId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.Reviews).WithOne(r => r.Product).HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Cascade);
            });

            // ProductFile
            modelBuilder.Entity<ProductFile>(entity =>
            {
                entity.ToTable("productfiles");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Product)
                      .WithMany(u => u.ProductFiles)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.File)
                      .WithMany(f => f.ProductFiles)
                      .HasForeignKey(e => e.FileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ProductColor
            modelBuilder.Entity<ProductColor>(entity =>
            {
                entity.ToTable("productcolors");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.ProductColors)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Color)
                      .WithMany(c => c.ProductColors)
                      .HasForeignKey(e => e.ColorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductSize
            modelBuilder.Entity<ProductSize>(entity =>
            {
                entity.ToTable("productsizes");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.ProductSizes)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Size)
                      .WithMany(s => s.ProductSizes)
                      .HasForeignKey(e => e.SizeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Favorite
            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.ToTable("favorites");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Favorites)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Favorites)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Cart
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("carts");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Carts)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Carts)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.SelectedColor)
                      .WithMany(c => c.CartsAsColor)
                      .HasForeignKey(e => e.SelectedColorId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.SelectedSize)
                      .WithMany(s => s.CartsAsSize)
                      .HasForeignKey(e => e.SelectedSizeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Address
            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("addresses");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Addresses)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.Property(e => e.Status).HasConversion<string>();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.DeliveryAddress)
                      .WithMany(a => a.Orders)
                      .HasForeignKey(e => e.DeliveryAddressId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(o => o.OrderItems).WithOne(oi => oi.Order).HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
            });

            // OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("orderitems");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.SelectedColor)
                      .WithMany(c => c.OrderItemsAsColor)
                      .HasForeignKey(e => e.SelectedColorId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.SelectedSize)
                      .WithMany(s => s.OrderItemsAsSize)
                      .HasForeignKey(e => e.SelectedSizeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Review
            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("reviews");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Verification
            modelBuilder.Entity<Verification>(entity =>
            {
                entity.ToTable("verifications");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Verifications)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // File
            modelBuilder.Entity<File>(entity =>
            {
                entity.ToTable("files");
                entity.HasKey(e => e.Id);
                entity.HasMany(f => f.ProductFiles).WithOne(pf => pf.File).HasForeignKey(pf => pf.FileId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}