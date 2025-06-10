using Microsoft.Extensions.DependencyInjection;
using Imperium.Data.Connections;
using Imperium.Data.Repositories;

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