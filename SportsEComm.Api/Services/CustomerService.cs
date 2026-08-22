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

    public CustomerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
            c.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase) && 
            c.SecretKeyHash == request.SecretKey);

        if (customer == null) return null;

        var token = $"token-{customer.Id}-{Guid.NewGuid():N}";
        return new LoginResponse(customer.Id, customer.Name, customer.Email, customer.Role, token);
    }
}
