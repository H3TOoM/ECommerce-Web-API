using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using ShopAPI.Data;
using ShopAPI.Helpers;
using ShopAPI.Repoistires;
using ShopAPI.Repoistires.Base;
using ShopAPI.Services;
using ShopAPI.Services.Base;

var builder = WebApplication.CreateBuilder(args);
const string AllowFrontendPolicy = "AllowFrontend";



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IMainRepository<>), typeof(MainRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IProductService, ProductService>()
        .AddScoped<IOrderService, OrderService>()
        .AddScoped<ICategoryService, CategoryService>()
        .AddScoped<IOrderItemService, OrderItemService>()
        .AddScoped<ICartItemService, CartItemService>()
        .AddScoped<IAccountService, AccountService>()
        .AddScoped<ICartService, CartService>()
        .AddScoped<IUserService, UserService>()
        .AddScoped<IAddressService, AddressService>()
        .AddScoped<ITokenService, TokenService>();

// Register Auto Mapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Configure JWT settings
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>() ?? new JwtSettings();

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

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Allow the standalone frontend (file:// or localhost) to talk to the API
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors(AllowFrontendPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
