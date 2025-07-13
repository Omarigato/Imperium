using AutoMapper;
using Imperium.Core.Models;
using Imperium.Service.DTOs.Address;
using Imperium.Service.DTOs.Auth;
using Imperium.Service.DTOs.Cart;
using Imperium.Service.DTOs.Client;
using Imperium.Service.DTOs.Dictionary;
using Imperium.Service.DTOs.Favorite;
using Imperium.Service.DTOs.Order;
using Imperium.Service.DTOs.Product;
using Imperium.Service.DTOs.User;
using System.Linq;

namespace Imperium.Service.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<User, AuthResponseDto>();
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.DeleteDate, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore()); // Пароль хешируется отдельно

            // Client mappings
            CreateMap<Client, ClientInfoDto>()
                .ForMember(dest => dest.Addresses, opt => opt.Ignore())
                .ForMember(dest => dest.Stats, opt => opt.Ignore());

            CreateMap<ClientRegisterDto, Client>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsPhoneVerified, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.DeleteDate, opt => opt.Ignore());

            CreateMap<UpdateClientDto, Client>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsPhoneVerified, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.DeleteDate, opt => opt.Ignore());

            // Dictionary mappings
            CreateMap<Dictionary, DictionaryDto>();

            // Address mappings
            CreateMap<Address, AddressDto>();
            CreateMap<CreateAddressDto, Address>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ClientId, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.DeleteDate, opt => opt.Ignore())
                .ForMember(dest => dest.Client, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore());

            // Product mappings
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.AvailableColors, opt => opt.MapFrom(src =>
                    src.ProductColors.Where(pc => pc.IsAvailable).Select(pc => pc.Color)))
                .ForMember(dest => dest.AvailableSizes, opt => opt.MapFrom(src =>
                    src.ProductSizes.Where(ps => ps.IsAvailable).Select(ps => ps.Size)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src =>
                    src.ProductFiles.Select(pf => pf.File.Url)));

            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeleteDate, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Material, opt => opt.Ignore())
                .ForMember(dest => dest.ProductFiles, opt => opt.Ignore())
                .ForMember(dest => dest.ProductColors, opt => opt.Ignore())
                .ForMember(dest => dest.ProductSizes, opt => opt.Ignore())
                .ForMember(dest => dest.Favorites, opt => opt.Ignore())
                .ForMember(dest => dest.Carts, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore());

            // Cart mappings
            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.ProductCode, opt => opt.Ignore())
                .ForMember(dest => dest.Price, opt => opt.Ignore())
                .ForMember(dest => dest.IsAvailable, opt => opt.Ignore())
                .ForMember(dest => dest.SelectedColorName, opt => opt.Ignore())
                .ForMember(dest => dest.SelectedSizeName, opt => opt.Ignore())
                .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryName, opt => opt.Ignore());

            CreateMap<AddToCartDto, Cart>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ClientId, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.DeleteDate, opt => opt.Ignore())
                .ForMember(dest => dest.Client, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.SelectedColor, opt => opt.Ignore())
                .ForMember(dest => dest.SelectedSize, opt => opt.Ignore());

            // Favorite mappings
            CreateMap<Favorite, FavoriteDto>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.ProductCode, opt => opt.Ignore())
                .ForMember(dest => dest.Price, opt => opt.Ignore())
                .ForMember(dest => dest.IsAvailable, opt => opt.Ignore())
                .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryName, opt => opt.Ignore());

            // Order mappings
            CreateMap<Order, OrderSummaryDto>()
                .ForMember(dest => dest.ItemsCount, opt => opt.Ignore())
                .ForMember(dest => dest.ClientName, opt => opt.Ignore())
                .ForMember(dest => dest.ClientPhone, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryAddress, opt => opt.Ignore());

            CreateMap<Order, OrderDetailsDto>()
                .ForMember(dest => dest.ClientName, opt => opt.Ignore())
                .ForMember(dest => dest.ClientPhone, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryAddress, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore());

            CreateMap<OrderItem, OrderItemDetailsDto>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.ProductCode, opt => opt.Ignore())
                .ForMember(dest => dest.SelectedColorName, opt => opt.Ignore())
                .ForMember(dest => dest.SelectedSizeName, opt => opt.Ignore())
                .ForMember(dest => dest.ProductImages, opt => opt.Ignore());

            CreateMap<CreateOrderFromCartDto, Order>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderNumber, opt => opt.Ignore())
                .ForMember(dest => dest.ClientId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryFee, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeleteDate, opt => opt.Ignore())
                .ForMember(dest => dest.Client, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryAddress, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore());
        }
    }
}