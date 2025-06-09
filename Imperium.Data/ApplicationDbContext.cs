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

            // User configurations
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Phone).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasConversion<string>();
            });

            // Dictionary configurations
            modelBuilder.Entity<Dictionary>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasOne(e => e.Parent)
                    .WithMany(e => e.Children)
                    .HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Product configurations
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasOne(e => e.Category)
                    .WithMany(e => e.ProductsAsCategory)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Material)
                    .WithMany(e => e.ProductsAsMaterial)
                    .HasForeignKey(e => e.MaterialId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductFile configurations
            modelBuilder.Entity<ProductFile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany(e => e.ProductFiles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.File)
                    .WithMany(e => e.ProductFiles)
                    .HasForeignKey(e => e.FileId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ProductColor configurations
            modelBuilder.Entity<ProductColor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Product)
                    .WithMany(e => e.ProductColors)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Color)
                    .WithMany(e => e.ProductColors)
                    .HasForeignKey(e => e.ColorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductSize configurations
            modelBuilder.Entity<ProductSize>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Product)
                    .WithMany(e => e.ProductSizes)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Size)
                    .WithMany(e => e.ProductSizes)
                    .HasForeignKey(e => e.SizeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Favorite configurations
            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
                entity.HasOne(e => e.User)
                    .WithMany(e => e.Favorites)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                    .WithMany(e => e.Favorites)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cart configurations
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
                entity.HasOne(e => e.User)
                    .WithMany(e => e.Carts)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                    .WithMany(e => e.Carts)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.SelectedColor)
                    .WithMany(e => e.CartsAsColor)
                    .HasForeignKey(e => e.SelectedColorId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.SelectedSize)
                    .WithMany(e => e.CartsAsSize)
                    .HasForeignKey(e => e.SelectedSizeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Address configurations
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany(e => e.Addresses)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Order configurations
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.Property(e => e.Status).HasConversion<string>();
                entity.HasOne(e => e.User)
                    .WithMany(e => e.Orders)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.DeliveryAddress)
                    .WithMany(e => e.Orders)
                    .HasForeignKey(e => e.DeliveryAddressId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderItem configurations
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Order)
                    .WithMany(e => e.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                    .WithMany(e => e.OrderItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.SelectedColor)
                    .WithMany(e => e.OrderItemsAsColor)
                    .HasForeignKey(e => e.SelectedColorId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.SelectedSize)
                    .WithMany(e => e.OrderItemsAsSize)
                    .HasForeignKey(e => e.SelectedSizeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Review configurations
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
                entity.HasOne(e => e.User)
                    .WithMany(e => e.Reviews)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Product)
                    .WithMany(e => e.Reviews)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Verification configurations
            modelBuilder.Entity<Verification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany(e => e.Verifications)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}