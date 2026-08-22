using Microsoft.AspNetCore.Mvc;
using SportsEComm.Api.DTOs;
using SportsEComm.Api.Services;

namespace SportsEComm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult> GetCart([FromQuery] int customerId)
    {
        try
        {
            var cart = await _cartService.GetCartByCustomerIdAsync(customerId);
            return Ok(cart);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> AddOrUpdateCartItem([FromBody] CartItemRequest request)
    {
        try
        {
            var cart = await _cartService.AddOrUpdateCartItemAsync(request);
            return Ok(cart);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{customerId}/items/{productId}")]
    public async Task<ActionResult> RemoveCartItem(int customerId, int productId)
    {
        try
        {
            var cart = await _cartService.RemoveCartItemAsync(customerId, productId);
            return Ok(cart);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
