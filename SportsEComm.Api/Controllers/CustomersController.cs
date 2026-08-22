using Microsoft.AspNetCore.Mvc;
using SportsEComm.Api.DTOs;
using SportsEComm.Api.Services;

namespace SportsEComm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetCustomers()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        return Ok(customers);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _customerService.AuthenticateAsync(request);
        if (response == null)
        {
            return Unauthorized(new { message = "Invalid email or secret key." });
        }

        return Ok(response);
    }
}
