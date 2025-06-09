using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Imperium.Core.Models;
using Imperium.Data.UnitOfWork;
using Imperium.Service.DTOs.Cart;

namespace Imperium.Service.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CartDto>> GetUserCartAsync(Guid userId)
        {
            var cartItems = await _unitOfWork.Carts.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<CartDto>>(cartItems);
        }

        public async Task<CartDto> AddToCartAsync(Guid userId, AddToCartDto addToCartDto)
        {
            var existingItem = await _unitOfWork.Carts.GetByUserAndProductAsync(userId, addToCartDto.ProductId);
            
            if (existingItem != null)
            {
                existingItem.Quantity += addToCartDto.Quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
                existingItem.Notes = addToCartDto.Notes;
                existingItem.SelectedColorId = addToCartDto.SelectedColorId;
                existingItem.SelectedSizeId = addToCartDto.SelectedSizeId;
                
                _unitOfWork.Carts.Update(existingItem);
                await _unitOfWork.SaveChangesAsync();
                
                return _mapper.Map<CartDto>(existingItem);
            }

            var cartItem = _mapper.Map<Cart>(addToCartDto);
            cartItem.UserId = userId;
            
            await _unitOfWork.Carts.AddAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();

            var addedItem = await _unitOfWork.Carts.GetByIdAsync(cartItem.Id);
            return _mapper.Map<CartDto>(addedItem!);
        }

        public async Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartId, int quantity)
        {
            var cartItem = await _unitOfWork.Carts.GetByIdAsync(cartId);
            if (cartItem == null || cartItem.UserId != userId)
                throw new KeyNotFoundException("Cart item not found");

            cartItem.Quantity = quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.Carts.Update(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CartDto>(cartItem);
        }

        public async Task RemoveFromCartAsync(Guid userId, Guid cartId)
        {
            var cartItem = await _unitOfWork.Carts.GetByIdAsync(cartId);
            if (cartItem == null || cartItem.UserId != userId)
                throw new KeyNotFoundException("Cart item not found");

            _unitOfWork.Carts.Remove(cartItem);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ClearCartAsync(Guid userId)
        {
            await _unitOfWork.Carts.ClearUserCartAsync(userId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<decimal> GetCartTotalAsync(Guid userId)
        {
            var cartItems = await _unitOfWork.Carts.GetByUserIdAsync(userId);
            return cartItems.Sum(item => item.Product.Price * item.Quantity);
        }
    }
}