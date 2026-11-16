using WebApplication1;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.Json;
using Scalar.AspNetCore;
using WebApplication1.Data;
using WebApplication1.Repositories.Contracts;
using WebApplication1.Repositories.Implementations;
using WebApplication1.Services;
using WebApplication1.Services.Contracts;
using WebApplication1.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

var tokenKey = builder.Configuration["AppSettings:Token"]
    ?? throw new InvalidOperationException("AppSettings:Token is missing.");
var issuer = builder.Configuration["AppSettings:Issuer"]!;
var audience = builder.Configuration["AppSettings:Audience"]!;

builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
builder.Services.AddOpenApi();

// Repository Pattern: controllers/services depend on contracts, not EF implementations.
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IReportsRepository, ReportsRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();

// Dependency Injection: business services receive their dependencies through constructors.
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ICategoriesService, CategoriesService>();
builder.Services.AddScoped<IDeliveriesService, DeliveriesService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<IProductImageService, ProductImageService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IAuthService, WebApplication1.AuthService>();

builder.Services.AddCors(options => options.AddPolicy("ConsoleClient", policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Use the same SQLite file as the MVC application (../WebApp/warehouse.db).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("ConsoleClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
