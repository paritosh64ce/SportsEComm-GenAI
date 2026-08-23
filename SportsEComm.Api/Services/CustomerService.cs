using SportsEComm.Api.DTOs;
using SportsEComm.Api.Repositories;

namespace SportsEComm.Api.Services;

public interface ICustomerService
{
    Task<IEnumerable<object>> GetAllCustomersAsync();
    Task<LoginResponse?> AuthenticateAsync(LoginRequest request);
}

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public CustomerService(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<IEnumerable<object>> GetAllCustomersAsync()
    {
        var customers = await _unitOfWork.Customers.GetAllAsync();
        return customers.Select(c => new { c.Id, c.Name, c.Email, c.Role, c.IsDemoLoginEnabled });
    }

    public async Task<LoginResponse?> AuthenticateAsync(LoginRequest request)
    {
        var customers = await _unitOfWork.Customers.GetAllAsync();
        var customer = customers.FirstOrDefault(c => 
            c.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

        if (customer == null || !customer.IsDemoLoginEnabled) return null;

        var isValid = _authService.VerifySecretKey(request.SecretKey, customer.SecretKeyHash);
        if (!isValid) return null;

        var token = _authService.GenerateJwtToken(customer);
        return new LoginResponse(customer.Id, customer.Name, customer.Email, customer.Role, token);
    }
}
