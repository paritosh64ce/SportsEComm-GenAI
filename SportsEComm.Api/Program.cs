using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SportsEComm.Api.Data;
using SportsEComm.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT
builder.Services.AddSwaggerGen(c =>
{
    // Attempt to resolve Microsoft.OpenApi types at runtime. If unavailable, skip security wiring to avoid throwing.
    Type openApiInfoType = null;
    Type openApiSecuritySchemeType = null;
    Type openApiReferenceType = null;
    Type openApiSecurityRequirementType = null;
    Type parameterLocationType = null;
    Type securitySchemeTypeEnum = null;
    Type referenceTypeEnum = null;

    try
    {
        var openApiAssembly = (System.Reflection.Assembly?)null;
        try
        {
            openApiAssembly = System.Reflection.Assembly.Load(new System.Reflection.AssemblyName("Microsoft.OpenApi"));
        }
        catch
        {
            openApiAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, "Microsoft.OpenApi", StringComparison.OrdinalIgnoreCase));
        }

        if (openApiAssembly != null)
        {
            openApiInfoType = openApiAssembly.GetType("Microsoft.OpenApi.Models.OpenApiInfo", false, true);
            openApiSecuritySchemeType = openApiAssembly.GetType("Microsoft.OpenApi.Models.OpenApiSecurityScheme", false, true);
            openApiReferenceType = openApiAssembly.GetType("Microsoft.OpenApi.Models.OpenApiReference", false, true);
            openApiSecurityRequirementType = openApiAssembly.GetType("Microsoft.OpenApi.Models.OpenApiSecurityRequirement", false, true);
            parameterLocationType = openApiAssembly.GetType("Microsoft.OpenApi.Models.ParameterLocation", false, true);
            securitySchemeTypeEnum = openApiAssembly.GetType("Microsoft.OpenApi.Models.SecuritySchemeType", false, true);
            referenceTypeEnum = openApiAssembly.GetType("Microsoft.OpenApi.Models.ReferenceType", false, true);
        }
    }
    catch (Exception ex)
    {
        // Log to console; do not throw from startup
        Console.WriteLine("Warning resolving Microsoft.OpenApi: " + ex.Message);
    }

    // If required types are not available, register a basic Swagger doc and return.
    if (openApiInfoType == null || openApiSecuritySchemeType == null || openApiReferenceType == null || openApiSecurityRequirementType == null || parameterLocationType == null || securitySchemeTypeEnum == null || referenceTypeEnum == null)
    {
        // Fallback: call SwaggerDoc with minimal info via dynamic invocation to avoid exceptions.
        try
        {
            // Create an anonymous info object compatible with SwaggerDoc invocation via reflection
            var info = new { Title = "SportsEComm API", Version = "v1" };
            c.GetType().GetMethod("SwaggerDoc")?.Invoke(c, new object[] { "v1", info });
        }
        catch
        {
            // ignore - we prefer app to run even if Swagger metadata is not fully wired
        }
        return;
    }

    // Create OpenApiInfo instance
    var openApiInfo = Activator.CreateInstance(openApiInfoType)!;
    openApiInfoType.GetProperty("Title")?.SetValue(openApiInfo, "SportsEComm API");
    openApiInfoType.GetProperty("Version")?.SetValue(openApiInfo, "v1");
    c.GetType().GetMethod("SwaggerDoc")?.Invoke(c, new object[] { "v1", openApiInfo });

    // Create security scheme
    var securityScheme = Activator.CreateInstance(openApiSecuritySchemeType)!;
    openApiSecuritySchemeType.GetProperty("Description")?.SetValue(securityScheme, "Enter 'Bearer <token>' (without quotes). Example: \"Bearer eyJ...\"");
    openApiSecuritySchemeType.GetProperty("Name")?.SetValue(securityScheme, "Authorization");
    var headerEnumValue = Enum.Parse(parameterLocationType!, "Header");
    openApiSecuritySchemeType.GetProperty("In")?.SetValue(securityScheme, headerEnumValue);
    var apiKeyEnumVal = Enum.Parse(securitySchemeTypeEnum!, "ApiKey");
    openApiSecuritySchemeType.GetProperty("Type")?.SetValue(securityScheme, apiKeyEnumVal);
    openApiSecuritySchemeType.GetProperty("Scheme")?.SetValue(securityScheme, "Bearer");
    openApiSecuritySchemeType.GetProperty("BearerFormat")?.SetValue(securityScheme, "JWT");

    c.GetType().GetMethod("AddSecurityDefinition")?.Invoke(c, new object[] { "Bearer", securityScheme });

    // Build security requirement with a reference to the scheme
    var openApiReference = Activator.CreateInstance(openApiReferenceType)!;
    openApiReferenceType.GetProperty("Type")?.SetValue(openApiReference, Enum.Parse(referenceTypeEnum!, "SecurityScheme"));
    openApiReferenceType.GetProperty("Id")?.SetValue(openApiReference, "Bearer");

    var schemeWithRef = Activator.CreateInstance(openApiSecuritySchemeType)!;
    openApiSecuritySchemeType.GetProperty("Reference")?.SetValue(schemeWithRef, openApiReference);
    openApiSecuritySchemeType.GetProperty("Scheme")?.SetValue(schemeWithRef, "Bearer");
    openApiSecuritySchemeType.GetProperty("Name")?.SetValue(schemeWithRef, "Authorization");
    openApiSecuritySchemeType.GetProperty("In")?.SetValue(schemeWithRef, headerEnumValue);

    var listOfString = Activator.CreateInstance(typeof(System.Collections.Generic.List<string>))!;
    var requirement = Activator.CreateInstance(openApiSecurityRequirementType)!;

    var addMethod = openApiSecurityRequirementType.GetMethod("Add", new[] { openApiSecuritySchemeType, typeof(System.Collections.Generic.IList<string>) });
    addMethod?.Invoke(requirement, new object[] { schemeWithRef, listOfString });

    c.GetType().GetMethod("AddSecurityRequirement")?.Invoke(c, new object[] { requirement });
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? "SportsECommSuperSecretKeyForJWTAuth2026!@#$%^&*()_+";
var issuer = jwtSettings["Issuer"] ?? "SportsECommApi";
var audience = jwtSettings["Audience"] ?? "SportsECommClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<SportsEComm.Api.Repositories.IUnitOfWork, SportsEComm.Api.Repositories.UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
builder.Services.AddDbContext<SportsECommContext>(options =>
    options.UseSqlServer(connectionString));

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
