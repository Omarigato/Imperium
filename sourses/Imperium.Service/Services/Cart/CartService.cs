using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Imperium.Core.Models;
using Imperium.Data.Repositories;
using Imperium.Service.DTOs.Cart;

namespace Imperium.Service.Services.Cart
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public CartService(ICartRepository cartRepository, IProductRepository productRepository, IMapper mapper)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CartDto>> GetUserCartAsync(Guid userId)
        {
            var cartItems = await _cartRepository.GetByUserIdWithDetailsAsync(userId);
            return _mapper.Map<IEnumerable<CartDto>>(cartItems);
        }

        public async Task<CartDto> AddToCartAsync(Guid userId, AddToCartDto addToCartDto)
        {
            var existingItem = await _cartRepository.GetByUserAndProductAsync(userId, addToCartDto.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += addToCartDto.Quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
                existingItem.Notes = addToCartDto.Notes;
                existingItem.SelectedColorId = addToCartDto.SelectedColorId;
                existingItem.SelectedSizeId = addToCartDto.SelectedSizeId;

                await _cartRepository.UpdateAsync(existingItem);
                return _mapper.Map<CartDto>(existingItem);
            }

            var cartItem = _mapper.Map<Core.Models.Cart>(addToCartDto);
            cartItem.UserId = userId;
            cartItem.CreatedAt = DateTime.UtcNow;
            cartItem.UpdatedAt = DateTime.UtcNow;

            var cartId = await _cartRepository.AddAsync(cartItem);
            cartItem.Id = cartId;

            return _mapper.Map<CartDto>(cartItem);
        }

        public async Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartId, int quantity)
        {
            var cartItem = await _cartRepository.GetByIdAsync(cartId);
            if (cartItem == null || cartItem.UserId != userId)
                throw new KeyNotFoundException("Cart item not found");

            cartItem.Quantity = quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;

            await _cartRepository.UpdateAsync(cartItem);
            return _mapper.Map<CartDto>(cartItem);
        }

        public async Task RemoveFromCartAsync(Guid userId, Guid cartId)
        {
            var cartItem = await _cartRepository.GetByIdAsync(cartId);
            if (cartItem == null || cartItem.UserId != userId)
                throw new KeyNotFoundException("Cart item not found");

            await _cartRepository.DeleteAsync(cartId);
        }

        public async Task ClearCartAsync(Guid userId)
        {
            await _cartRepository.ClearUserCartAsync(userId);
        }

        public async Task<decimal> GetCartTotalAsync(Guid userId)
        {
            var cartItems = await _cartRepository.GetByUserIdWithDetailsAsync(userId);
            decimal total = 0;

            foreach (var item in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    total += product.Price * item.Quantity;
                }
            }

            return total;
        }
    }
}