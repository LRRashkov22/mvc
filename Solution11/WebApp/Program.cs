using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.Data;
using WebApplication1.Services;
using WebApplication1.Services.Implementations;
using WebApplication1.Services.Contracts;
using Scalar.AspNetCore;
using WebApplication1.Repositories.Implementations;
using WebApplication1.Repositories.Contracts;

using Serilog;
using WebApplication1.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Register the database context using SQLite
builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// OpenAPI document generation (served at /openapi/v1.json)
builder.Services.AddOpenApi();

// Cookie authentication (used by AccountController.Login)
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// File upload settings (appsettings.json -> "FileStorage")
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();

// Register application services for dependency injection
builder.Services.AddScoped<ICategoriesService, CategoriesService>();
builder.Services.AddScoped<IDeliveriesService, DeliveriesService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductImageService, ProductImageService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Register repository services for dependency injection
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend",
        policy => policy
            .WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

// Existing services
builder.Services.AddControllersWithViews();

// Keep your existing DbContext and service registrations here.

var app = builder.Build();

// Global exception handling
app.UseExceptionHandler("/Error/500");

// Log unhandled exceptions
app.UseMiddleware<ExceptionLoggingMiddleware>();

// Custom status code pages (404, etc.)
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseCors("frontend");

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapOpenApi();            // serves /openapi/v1.json
app.MapScalarApiReference(); // UI at /scalar

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();