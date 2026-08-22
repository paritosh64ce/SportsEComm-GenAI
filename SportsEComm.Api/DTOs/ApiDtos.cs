namespace SportsEComm.Api.DTOs;

public record LoginRequest(string Email, string SecretKey);
public record LoginResponse(int CustomerId, string Name, string Email, string Role, string Token);

public record CartItemRequest(int CustomerId, int ProductId, int Quantity);

public record PlaceOrderRequest(int CustomerId);
