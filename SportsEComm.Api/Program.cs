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
    // Build OpenAPI types by reflection to avoid a direct compile-time dependency on Microsoft.OpenApi.Models
    var openApiAssemblyName = "Microsoft.OpenApi";

    var openApiInfoType = Type.GetType("Microsoft.OpenApi.Models.OpenApiInfo, " + openApiAssemblyName);
    var openApiSecuritySchemeType = Type.GetType("Microsoft.OpenApi.Models.OpenApiSecurityScheme, " + openApiAssemblyName);
    var openApiReferenceType = Type.GetType("Microsoft.OpenApi.Models.OpenApiReference, " + openApiAssemblyName);
    var openApiSecurityRequirementType = Type.GetType("Microsoft.OpenApi.Models.OpenApiSecurityRequirement, " + openApiAssemblyName);
    var parameterLocationType = Type.GetType("Microsoft.OpenApi.Models.ParameterLocation, " + openApiAssemblyName);
    var securitySchemeTypeEnum = Type.GetType("Microsoft.OpenApi.Models.SecuritySchemeType, " + openApiAssemblyName);
    var referenceTypeEnum = Type.GetType("Microsoft.OpenApi.Models.ReferenceType, " + openApiAssemblyName);

    // Create and set OpenApiInfo
    var openApiInfo = Activator.CreateInstance(openApiInfoType);
    openApiInfoType.GetProperty("Title")?.SetValue(openApiInfo, "SportsEComm API");
    openApiInfoType.GetProperty("Version")?.SetValue(openApiInfo, "v1");
    c.GetType().GetMethod("SwaggerDoc")?.Invoke(c, new object[] { "v1", openApiInfo });

    // Create security scheme
    var securityScheme = Activator.CreateInstance(openApiSecuritySchemeType);
    openApiSecuritySchemeType.GetProperty("Description")?.SetValue(securityScheme, "Enter 'Bearer <token>' (without quotes). Example: \"Bearer eyJ...\"");
    openApiSecuritySchemeType.GetProperty("Name")?.SetValue(securityScheme, "Authorization");
    var headerEnumValue = Enum.Parse(parameterLocationType, "Header");
    openApiSecuritySchemeType.GetProperty("In")?.SetValue(securityScheme, headerEnumValue);
    var apiKeyEnumVal = Enum.Parse(securitySchemeTypeEnum, "ApiKey");
    openApiSecuritySchemeType.GetProperty("Type")?.SetValue(securityScheme, apiKeyEnumVal);
    openApiSecuritySchemeType.GetProperty("Scheme")?.SetValue(securityScheme, "Bearer");
    openApiSecuritySchemeType.GetProperty("BearerFormat")?.SetValue(securityScheme, "JWT");

    c.GetType().GetMethod("AddSecurityDefinition")?.Invoke(c, new object[] { "Bearer", securityScheme });

    // Build security requirement with a reference to the scheme
    var openApiReference = Activator.CreateInstance(openApiReferenceType);
    openApiReferenceType.GetProperty("Type")?.SetValue(openApiReference, Enum.Parse(referenceTypeEnum, "SecurityScheme"));
    openApiReferenceType.GetProperty("Id")?.SetValue(openApiReference, "Bearer");

    var schemeWithRef = Activator.CreateInstance(openApiSecuritySchemeType);
    openApiSecuritySchemeType.GetProperty("Reference")?.SetValue(schemeWithRef, openApiReference);
    openApiSecuritySchemeType.GetProperty("Scheme")?.SetValue(schemeWithRef, "Bearer");
    openApiSecuritySchemeType.GetProperty("Name")?.SetValue(schemeWithRef, "Authorization");
    openApiSecuritySchemeType.GetProperty("In")?.SetValue(schemeWithRef, headerEnumValue);

    var listOfString = Activator.CreateInstance(typeof(System.Collections.Generic.List<string>));
    var requirement = Activator.CreateInstance(openApiSecurityRequirementType);

    // OpenApiSecurityRequirement implements IDictionary<OpenApiSecurityScheme, IList<string>>; invoke Add(key, value)
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
