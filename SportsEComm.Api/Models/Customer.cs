namespace SportsEComm.Api.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SecretKeyHash { get; set; } = string.Empty; // Store hashed/secured secret key or direct demo key representation
    public bool IsDemoLoginEnabled { get; set; }
    public string Role { get; set; } = "Customer";
}
