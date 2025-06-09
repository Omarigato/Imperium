using Imperium.Service.DTOs.Cart;

namespace Imperium.Service.Services
{
    public interface ICartService
    {
        Task<IEnumerable<CartDto>> GetUserCartAsync(Guid userId);
        Task<CartDto> AddToCartAsync(Guid userId, AddToCartDto addToCartDto);
        Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartId, int quantity);
        Task RemoveFromCartAsync(Guid userId, Guid cartId);
        Task ClearCartAsync(Guid userId);
        Task<decimal> GetCartTotalAsync(Guid userId);
    }
}