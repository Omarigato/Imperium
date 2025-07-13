using Microsoft.Extensions.DependencyInjection;
using Imperium.Data.Connections;
using Imperium.Data.Repositories.Client;
using Imperium.Data.Repositories.Address;
using Imperium.Data.Repositories.Cart;
using Imperium.Data.Repositories.Dictionary;
using Imperium.Data.Repositories.Favorite;
using Imperium.Data.Repositories.File;
using Imperium.Data.Repositories.Log;
using Imperium.Data.Repositories.Order;
using Imperium.Data.Repositories.OrderItem;
using Imperium.Data.Repositories.ProductColor;
using Imperium.Data.Repositories.ProductFile;
using Imperium.Data.Repositories.Product;
using Imperium.Data.Repositories.ProductSize;
using Imperium.Data.Repositories.Review;
using Imperium.Data.Repositories.User;
using Imperium.Data.Repositories.Verification;

namespace Imperium.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataLayer(this IServiceCollection services)
        {
            // Connection Factory
            services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IDictionaryRepository, DictionaryRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IVerificationRepository, VerificationRepository>();
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddScoped<IProductFileRepository, ProductFileRepository>();
            services.AddScoped<IProductColorRepository, ProductColorRepository>();
            services.AddScoped<IProductSizeRepository, ProductSizeRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();

            return services;
        }
    }
}