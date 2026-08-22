using Microsoft.EntityFrameworkCore;
using SportsEComm.Api.DTOs;
using SportsEComm.Api.Models;
using SportsEComm.Api.Repositories;

namespace SportsEComm.Api.Services;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);
    Task<Order> PlaceOrderAsync(PlaceOrderRequest request);
}

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly SportsEComm.Api.Data.SportsECommContext _context;

    public OrderService(IUnitOfWork unitOfWork, SportsEComm.Api.Data.SportsECommContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders;
    }

    public async Task<Order> PlaceOrderAsync(PlaceOrderRequest request)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId);

        if (cart == null || !cart.Items.Any())
        {
            throw new InvalidOperationException("Cart is empty or does not exist.");
        }

        decimal totalAmount = cart.Items.Sum(i => (i.Product?.Price ?? 0) * i.Quantity);

        var order = new Order
        {
            CustomerId = request.CustomerId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = totalAmount,
            Status = "Confirmed",
            Items = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Unknown",
                UnitPrice = i.Product?.Price ?? 0,
                Quantity = i.Quantity
            }).ToList()
        };

        // Reduce stock
        foreach (var cartItem in cart.Items)
        {
            if (cartItem.Product != null)
            {
                cartItem.Product.Stock -= cartItem.Quantity;
                _unitOfWork.Products.Update(cartItem.Product);
            }
        }

        // Clear cart items
        foreach (var cartItem in cart.Items.ToList())
        {
            _unitOfWork.CartItems.Remove(cartItem);
        }

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.CompleteAsync();

        return order;
    }
}
