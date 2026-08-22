using Microsoft.AspNetCore.Mvc;
using SportsEComm.Api.DTOs;
using SportsEComm.Api.Services;

namespace SportsEComm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult> GetOrders([FromQuery] int customerId)
    {
        try
        {
            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        try
        {
            var order = await _orderService.PlaceOrderAsync(request);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
