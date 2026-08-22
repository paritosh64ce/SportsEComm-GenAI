using SportsEComm.Api.Data;
using SportsEComm.Api.Models;

namespace SportsEComm.Api.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly SportsECommContext _context;

    public IRepository<Product> Products { get; }
    public IRepository<Customer> Customers { get; }
    public IRepository<Cart> Carts { get; }
    public IRepository<CartItem> CartItems { get; }
    public IRepository<Order> Orders { get; }

    public UnitOfWork(SportsECommContext context)
    {
        _context = context;
        Products = new Repository<Product>(_context);
        Customers = new Repository<Customer>(_context);
        Carts = new Repository<Cart>(_context);
        CartItems = new Repository<CartItem>(_context);
        Orders = new Repository<Order>(_context);
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
