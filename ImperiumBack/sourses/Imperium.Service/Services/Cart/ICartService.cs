using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Imperium.Service.DTOs.Cart;

namespace Imperium.Service.Services.Cart
{
    public interface ICartService
    {
        Task<CartSummaryDto> GetClientCartAsync(Guid clientId);
        Task<CartDto> AddToCartAsync(Guid clientId, AddToCartDto addToCartDto);
        Task<CartDto?> UpdateCartItemAsync(Guid clientId, Guid cartId, UpdateCartItemDto updateDto);
        Task<bool> RemoveFromCartAsync(Guid clientId, Guid cartId);
        Task<bool> ClearCartAsync(Guid clientId);
        Task<CartSummaryDto> GetCartSummaryAsync(Guid clientId);
        Task<bool> IsProductInCartAsync(Guid clientId, Guid productId);
        Task<int> GetCartItemsCountAsync(Guid clientId);
    }
}