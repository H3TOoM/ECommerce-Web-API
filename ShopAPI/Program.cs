using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using ShopAPI.Data;
using ShopAPI.Helpers;
using ShopAPI.Middleware;
using ShopAPI.Repoistires;
using ShopAPI.Repoistires.Base;
using ShopAPI.Services;
using ShopAPI.Services.Base;
using ShopAPI.Validators;

var builder = WebApplication.CreateBuilder(args);
const string AllowFrontendPolicy = "AllowFrontend";

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/shopapi-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repository and Unit of Work
builder.Services.AddScoped(typeof(IMainRepository<>), typeof(MainRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Services
builder.Services
    .AddScoped<IProductService, ProductService>()
    .AddScoped<IOrderService, OrderService>()
    .AddScoped<ICategoryService, CategoryService>()
    .AddScoped<IOrderItemService, OrderItemService>()
    .AddScoped<ICartItemService, CartItemService>()
    .AddScoped<IAccountService, AccountService>()
    .AddScoped<ICartService, CartService>()
    .AddScoped<IUserService, UserService>()
    .AddScoped<IAddressService, AddressService>()
    .AddScoped<ITokenService, TokenService>();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ProductCreateDtoValidator>();

// Register Memory Cache for caching
builder.Services.AddMemoryCache();

// Configure JWT settings
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>() ?? new JwtSettings();

// Configure Authentication
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
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey))
    };
});

builder.Services.AddAuthorization();

// Add API controllers
builder.Services.AddControllers();

// Configure OpenAPI/Swagger
builder.Services.AddOpenApi();

// Configure CORS with stricter policy for production
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowFrontendPolicy, policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Apply global exception handling middleware (must be early in pipeline)
app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();

app.UseCors(AllowFrontendPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
