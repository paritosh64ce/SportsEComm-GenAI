using Microsoft.EntityFrameworkCore;
using SportsEComm.Api.Models;

namespace SportsEComm.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(SportsECommContext context)
    {
        await context.Database.EnsureCreatedAsync();

        // Normalize existing customers: hash any plaintext secret keys in the database so secrets are never stored in cleartext.
        var _passwordHasher_for_existing = new Microsoft.AspNetCore.Identity.PasswordHasher<Customer>();
        var existingCustomers = await context.Customers.ToListAsync();
        if (existingCustomers.Any())
        {
            var updatedExisting = false;
            foreach (var ec in existingCustomers)
            {
                if (!string.IsNullOrWhiteSpace(ec.SecretKeyHash) && !ec.SecretKeyHash.StartsWith("AQAAAA"))
                {
                    ec.SecretKeyHash = _passwordHasher_for_existing.HashPassword(ec, ec.SecretKeyHash);
                    updatedExisting = true;
                }
            }
            if (updatedExisting) await context.SaveChangesAsync();
        }

        if (await context.Products.AnyAsync())
        {
            return; // Already seeded
        }

        // 1. Seed Products (Premium Cricket Equipment)
        var products = new List<Product>
        {
            new Product
            {
                Name = "MSD Signature English Willow Cricket Bat",
                Description = "Grade 1+ seasoned English willow bat, massive sweet spot, favored by power hitters.",
                Price = 28999.00m,
                Stock = 15,
                ImageUrl = "/images/msd-bat.jpg"
            },
            new Product
            {
                Name = "Master Blaster Pro Cricket Bat",
                Description = "Hand-crafted premium willow with exceptional balance and pickup for classical stroke play.",
                Price = 32499.00m,
                Stock = 10,
                ImageUrl = "/images/master-blaster-bat.jpg"
            },
            new Product
            {
                Name = "World Cup Edition Leather Cricket Ball (Pack of 6)",
                Description = "Four-piece alum tanned wax polished red leather balls designed for professional matches.",
                Price = 4500.00m,
                Stock = 50,
                ImageUrl = "/images/wc-balls.jpg"
            },
            new Product
            {
                Name = "ProKeeper Cricket Wicketkeeping Gloves",
                Description = "Premium aniline leather palms with octopus rubber grip and high-density foam protection.",
                Price = 5999.00m,
                Stock = 25,
                ImageUrl = "/images/wk-gloves.jpg"
            },
            new Product
            {
                Name = "TurboFlex Full Cricket Batting Pads",
                Description = "Lightweight ultra-protective ergonomic leggards with high-density cane reinforcement.",
                Price = 6499.00m,
                Stock = 20,
                ImageUrl = "/images/batting-pads.jpg"
            },
            new Product
            {
                Name = "StumpVision Pro Cricket Helmet",
                Description = "Titanium grill steel-reinforced helmet complying with latest safety British standards.",
                Price = 7199.00m,
                Stock = 18,
                ImageUrl = "/images/helmet.jpg"
            },
            new Product
            {
                Name = "All-Rounder Heavy-Duty Cricket Kit Bag",
                Description = "Wheeled heavy-duty compartmentalized bag with shoe tunnel and thermal bat pocket.",
                Price = 8999.00m,
                Stock = 12,
                ImageUrl = "/images/kitbag.jpg"
            }
        };

        context.Products.AddRange(products);

        // 2. Seed Customers (2011 India World Cup Squad Highlights)
        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Customer>();

        // Seed demo customers (secrets specified here are temporary plaintexts and will be hashed before saving)
        var customers = new List<Customer>
        {
            new Customer { Name = "MS Dhoni", Email = "ms.dhoni@teamindia2011.com", SecretKeyHash = "dhoni7#cup", IsDemoLoginEnabled = true, Role = "Captain" },
            new Customer { Name = "Sachin Tendulkar", Email = "sachin.tendulkar@teamindia2011.com", SecretKeyHash = "sachin10#master", IsDemoLoginEnabled = true, Role = "Batsman" },
            new Customer { Name = "Virat Kohli", Email = "virat.kohli@teamindia2011.com", SecretKeyHash = "kohli18#chase", IsDemoLoginEnabled = true, Role = "Batsman" },
            new Customer { Name = "Yuvraj Singh", Email = "yuvraj.singh@teamindia2011.com", SecretKeyHash = "yuvraj12#champ", IsDemoLoginEnabled = true, Role = "AllRounder" },
            new Customer { Name = "Gautam Gambhir", Email = "gautam.gambhir@teamindia2011.com", SecretKeyHash = "squad2011", IsDemoLoginEnabled = false, Role = "Batsman" },
            new Customer { Name = "Virender Sehwag", Email = "virender.sehwag@teamindia2011.com", SecretKeyHash = "squad2011", IsDemoLoginEnabled = false, Role = "Batsman" },
            new Customer { Name = "Zaheer Khan", Email = "zaheer.khan@teamindia2011.com", SecretKeyHash = "squad2011", IsDemoLoginEnabled = false, Role = "Bowler" },
            new Customer { Name = "Harbhajan Singh", Email = "harbhajan.singh@teamindia2011.com", SecretKeyHash = "squad2011", IsDemoLoginEnabled = false, Role = "Bowler" },
            new Customer { Name = "Suresh Raina", Email = "suresh.raina@teamindia2011.com", SecretKeyHash = "squad2011", IsDemoLoginEnabled = false, Role = "Batsman" },
            new Customer { Name = "Munaf Patel", Email = "munaf.patel@teamindia2011.com", SecretKeyHash = "squad2011", IsDemoLoginEnabled = false, Role = "Bowler" },
            new Customer { Name = "S Sreesanth", Email = "s.sreesanth@teamindia2011.com", SecretKeyHash = "squad2011", IsDemoLoginEnabled = false, Role = "Bowler" }
        };

        // Ensure seeded plaintext secrets are hashed before insertion
        foreach (var c in customers)
        {
            if (!string.IsNullOrWhiteSpace(c.SecretKeyHash) && !c.SecretKeyHash.StartsWith("AQAAAA"))
            {
                c.SecretKeyHash = passwordHasher.HashPassword(c, c.SecretKeyHash);
            }
        }

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        // 3. Seed an initial order for MS Dhoni (Customer ID 1)
        var dhoni = await context.Customers.FirstOrDefaultAsync(c => c.Email == "ms.dhoni@teamindia2011.com");
        var msdBat = await context.Products.FirstOrDefaultAsync(p => p.Name.Contains("MSD Signature"));

        if (dhoni != null && msdBat != null)
        {
            var initialOrder = new Order
            {
                CustomerId = dhoni.Id,
                OrderDate = DateTime.UtcNow.AddDays(-2),
                TotalAmount = msdBat.Price,
                Status = OrderStatus.Delivered,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = msdBat.Id,
                        ProductName = msdBat.Name,
                        UnitPrice = msdBat.Price,
                        Quantity = 1
                    }
                }
            };
            context.Orders.Add(initialOrder);
            await context.SaveChangesAsync();
        }
    }
}
