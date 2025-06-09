using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Imperium.Service.Services;
using Imperium.Service.DTOs.Cart;

namespace Imperium.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartDto>>> GetCart()
        {
            var userId = GetCurrentUserId();
            var cartItems = await _cartService.GetUserCartAsync(userId);
            return Ok(cartItems);
        }

        [HttpPost]
        public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartDto addToCartDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var cartItem = await _cartService.AddToCartAsync(userId, addToCartDto);
                return Ok(cartItem);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{cartId}")]
        public async Task<ActionResult<CartDto>> UpdateCartItem(Guid cartId, [FromBody] int quantity)
        {
            try
            {
                var userId = GetCurrentUserId();
                var cartItem = await _cartService.UpdateCartItemAsync(userId, cartId, quantity);
                return Ok(cartItem);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{cartId}")]
        public async Task<ActionResult> RemoveFromCart(Guid cartId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _cartService.RemoveFromCartAsync(userId, cartId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete]
        public async Task<ActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            await _cartService.ClearCartAsync(userId);
            return NoContent();
        }

        [HttpGet("total")]
        public async Task<ActionResult<decimal>> GetCartTotal()
        {
            var userId = GetCurrentUserId();
            var total = await _cartService.GetCartTotalAsync(userId);
            return Ok(total);
        }
    }
}