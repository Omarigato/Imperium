using AutoMapper;
using Imperium.Data.Repositories.Cart;
using Imperium.Data.Repositories.Dictionary;
using Imperium.Data.Repositories.File;
using Imperium.Data.Repositories.Product;
using Imperium.Service.DTOs.Cart;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Cart
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IFileRepository fileRepository,
            IDictionaryRepository dictionaryRepository,
            IMapper mapper,
            ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _fileRepository = fileRepository;
            _dictionaryRepository = dictionaryRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CartSummaryDto> GetClientCartAsync(Guid clientId)
        {
            try
            {
                var cartItems = await _cartRepository.GetByClientIdWithDetailsAsync(clientId);
                var cartDtos = new List<CartDto>();

                foreach (var item in cartItems)
                {
                    var product = await _productRepository.GetWithDetailsAsync(item.ProductId);
                    if (product != null)
                    {
                        var productImages = await _fileRepository.GetByProductIdAsync(product.Id);

                        var selectedColor = item.SelectedColorId.HasValue
                            ? await _dictionaryRepository.GetByIdAsync(item.SelectedColorId.Value)
                            : null;

                        var selectedSize = item.SelectedSizeId.HasValue
                            ? await _dictionaryRepository.GetByIdAsync(item.SelectedSizeId.Value)
                            : null;

                        cartDtos.Add(new CartDto
                        {
                            Id = item.Id,
                            ProductId = product.Id,
                            Quantity = item.Quantity,
                            Notes = item.Notes,
                            CreateDate = item.CreateDate,
                            ProductName = product.NameRu,
                            ProductCode = product.Code,
                            Price = product.Price,
                            IsAvailable = product.IsAvailable,
                            SelectedColorName = selectedColor?.NameRu,
                            SelectedSizeName = selectedSize?.NameRu,
                            SelectedColorId = item.SelectedColorId,
                            SelectedSizeId = item.SelectedSizeId,
                            ProductImages = productImages.Select(f => f.Url).ToList(),
                            CategoryName = product.Category?.NameRu ?? ""
                        });
                    }
                }

                return CreateCartSummary(cartDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for client {ClientId}", clientId);
                return new CartSummaryDto();
            }
        }

        public async Task<CartDto> AddToCartAsync(Guid clientId, AddToCartDto addToCartDto)
        {
            try
            {
                // Проверяем существование товара
                var product = await _productRepository.GetWithDetailsAsync(addToCartDto.ProductId);
                if (product == null || !product.IsAvailable)
                {
                    throw new InvalidOperationException("Товар не найден или недоступен");
                }

                // Проверяем, есть ли уже такой товар в корзине с теми же параметрами
                var existingItem = await FindExistingCartItem(clientId, addToCartDto);

                if (existingItem != null)
                {
                    // Обновляем количество существующего товара
                    existingItem.Quantity += addToCartDto.Quantity;
                    existingItem.Notes = addToCartDto.Notes; // Обновляем заметки

                    await _cartRepository.Update(existingItem);
                    return await BuildCartDto(existingItem);
                }
                else
                {
                    // Создаем новый элемент корзины
                    var cartItem = new Core.Models.Cart
                    {
                        ClientId = clientId,
                        ProductId = addToCartDto.ProductId,
                        Quantity = addToCartDto.Quantity,
                        SelectedColorId = addToCartDto.SelectedColorId,
                        SelectedSizeId = addToCartDto.SelectedSizeId,
                        Notes = addToCartDto.Notes,
                        CreateDate = DateTime.UtcNow
                    };

                    await _cartRepository.Insert(cartItem);
                    return await BuildCartDto(cartItem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductId} to cart for client {ClientId}",
                    addToCartDto.ProductId, clientId);
                throw;
            }
        }

        public async Task<CartDto?> UpdateCartItemAsync(Guid clientId, Guid cartId, UpdateCartItemDto updateDto)
        {
            try
            {
                var cartItem = await _cartRepository.GetByIdAsync(cartId);
                if (cartItem == null || cartItem.ClientId != clientId)
                {
                    return null;
                }

                cartItem.Quantity = updateDto.Quantity;
                cartItem.SelectedColorId = updateDto.SelectedColorId;
                cartItem.SelectedSizeId = updateDto.SelectedSizeId;
                cartItem.Notes = updateDto.Notes;

                await _cartRepository.Update(cartItem);
                return await BuildCartDto(cartItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {CartId} for client {ClientId}", cartId, clientId);
                return null;
            }
        }

        public async Task<bool> RemoveFromCartAsync(Guid clientId, Guid cartId)
        {
            try
            {
                var cartItem = await _cartRepository.GetByIdAsync(cartId);
                if (cartItem == null || cartItem.ClientId != clientId)
                {
                    return false;
                }

                cartItem.DeleteDate = DateTime.UtcNow;
                await _cartRepository.Update(cartItem);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item {CartId} for client {ClientId}", cartId, clientId);
                return false;
            }
        }

        public async Task<bool> ClearCartAsync(Guid clientId)
        {
            try
            {
                return await _cartRepository.ClearClientCartAsync(clientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for client {ClientId}", clientId);
                return false;
            }
        }

        public async Task<CartSummaryDto> GetCartSummaryAsync(Guid clientId)
        {
            return await GetClientCartAsync(clientId);
        }

        public async Task<bool> IsProductInCartAsync(Guid clientId, Guid productId)
        {
            try
            {
                var cartItem = await _cartRepository.GetByClientAndProductAsync(clientId, productId);
                return cartItem != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product {ProductId} is in cart for client {ClientId}",
                    productId, clientId);
                return false;
            }
        }

        public async Task<int> GetCartItemsCountAsync(Guid clientId)
        {
            try
            {
                var cartItems = await _cartRepository.GetByClientIdAsync(clientId);
                return cartItems.Sum(c => c.Quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart items count for client {ClientId}", clientId);
                return 0;
            }
        }

        private async Task<Core.Models.Cart?> FindExistingCartItem(Guid clientId, AddToCartDto addToCartDto)
        {
            var cartItems = await _cartRepository.GetByClientIdAsync(clientId);

            return cartItems.FirstOrDefault(c =>
                c.ProductId == addToCartDto.ProductId &&
                c.SelectedColorId == addToCartDto.SelectedColorId &&
                c.SelectedSizeId == addToCartDto.SelectedSizeId);
        }

        private async Task<CartDto> BuildCartDto(Core.Models.Cart cartItem)
        {
            var product = await _productRepository.GetWithDetailsAsync(cartItem.ProductId);
            var productImages = await _fileRepository.GetByProductIdAsync(cartItem.ProductId);

            var selectedColor = cartItem.SelectedColorId.HasValue
                ? await _dictionaryRepository.GetByIdAsync(cartItem.SelectedColorId.Value)
                : null;

            var selectedSize = cartItem.SelectedSizeId.HasValue
                ? await _dictionaryRepository.GetByIdAsync(cartItem.SelectedSizeId.Value)
                : null;

            return new CartDto
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                Notes = cartItem.Notes,
                CreateDate = cartItem.CreateDate,
                ProductName = product?.NameRu ?? "",
                ProductCode = product?.Code ?? "",
                Price = product?.Price ?? 0,
                IsAvailable = product?.IsAvailable ?? false,
                SelectedColorName = selectedColor?.NameRu,
                SelectedSizeName = selectedSize?.NameRu,
                SelectedColorId = cartItem.SelectedColorId,
                SelectedSizeId = cartItem.SelectedSizeId,
                ProductImages = productImages.Select(f => f.Url).ToList(),
                CategoryName = product?.Category?.NameRu ?? ""
            };
        }

        private static CartSummaryDto CreateCartSummary(List<CartDto> cartItems)
        {
            var itemsCount = cartItems.Count;
            var totalQuantity = cartItems.Sum(c => c.Quantity);
            var subTotal = cartItems.Sum(c => c.TotalPrice);

            // Логика расчета доставки (можно настроить)
            var deliveryFee = subTotal >= 50000 ? 0 : 2000; // Бесплатная доставка от 50,000 тенге

            return new CartSummaryDto
            {
                ItemsCount = itemsCount,
                TotalQuantity = totalQuantity,
                SubTotal = subTotal,
                DeliveryFee = deliveryFee,
                TotalAmount = subTotal + deliveryFee,
                Items = cartItems.OrderByDescending(c => c.CreateDate).ToList()
            };
        }
    }
}