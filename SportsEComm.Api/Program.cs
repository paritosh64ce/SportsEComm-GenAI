using Microsoft.EntityFrameworkCore;
using SportsEComm.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<SportsEComm.Api.Repositories.IUnitOfWork, SportsEComm.Api.Repositories.UnitOfWork>();
builder.Services.AddScoped<SportsEComm.Api.Services.IProductService, SportsEComm.Api.Services.ProductService>();
builder.Services.AddScoped<SportsEComm.Api.Services.ICustomerService, SportsEComm.Api.Services.CustomerService>();
builder.Services.AddScoped<SportsEComm.Api.Services.ICartService, SportsEComm.Api.Services.CartService>();
builder.Services.AddScoped<SportsEComm.Api.Services.IOrderService, SportsEComm.Api.Services.OrderService>();

builder.Services.AddDbContext<SportsECommContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
        "Server=(localdb)\\mssqllocaldb;Database=SportsECommDb;Trusted_Connection=True;MultipleActiveResultSets=true"));

var app = builder.Build();

// Initialize and seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SportsECommContext>();
        await DbInitializer.InitializeAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
