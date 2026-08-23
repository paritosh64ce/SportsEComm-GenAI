using Microsoft.EntityFrameworkCore;
using SportsEComm.Api.DTOs;
using SportsEComm.Api.Models;
using SportsEComm.Api.Repositories;

namespace SportsEComm.Api.Services;

public interface ICartService
{
    Task<Cart> GetCartByCustomerIdAsync(int customerId);
    Task<Cart> AddOrUpdateCartItemAsync(int customerId, CartItemRequest request);
    Task<Cart> RemoveCartItemAsync(int customerId, int productId);
}

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Data.SportsECommContext _context;

    public CartService(IUnitOfWork unitOfWork, Data.SportsECommContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Cart> GetCartByCustomerIdAsync(int customerId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (cart == null)
        {
            cart = new Cart { CustomerId = customerId };
            await _unitOfWork.Carts.AddAsync(cart);
            await _unitOfWork.CompleteAsync();
        }

        return cart;
    }

    public async Task<Cart> AddOrUpdateCartItemAsync(int customerId, CartItemRequest request)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (cart == null)
        {
            cart = new Cart { CustomerId = customerId };
            await _unitOfWork.Carts.AddAsync(cart);
            await _unitOfWork.CompleteAsync();
        }

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product == null) throw new KeyNotFoundException("Product not found.");

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (request.Quantity <= 0)
        {
            if (existingItem != null)
            {
                _unitOfWork.CartItems.Remove(existingItem);
            }
        }
        else
        {
            if (existingItem != null)
            {
                existingItem.Quantity = request.Quantity;
                _unitOfWork.CartItems.Update(existingItem);
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                });
            }
        }

        await _unitOfWork.CompleteAsync();
        return await GetCartByCustomerIdAsync(customerId);
    }

    public async Task<Cart> RemoveCartItemAsync(int customerId, int productId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (cart == null) throw new KeyNotFoundException("Cart not found.");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _unitOfWork.CartItems.Remove(item);
            await _unitOfWork.CompleteAsync();
        }

        return await GetCartByCustomerIdAsync(customerId);
    }
}
