using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data;

public class WarehouseDbContext : DbContext
{
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
        : base(options)
    {
    }
    public DbSet<User> User => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InventoryItem> Inventory => Set<InventoryItem>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.CategoryId);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(x => x.ProductId);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Price).HasColumnType("decimal(10,2)");
            entity.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("ProductImages");
            entity.HasKey(x => x.ProductImageId);
            entity.Property(x => x.FileName).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ThumbFileName).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OriginalFileName).HasMaxLength(100);
            entity.HasIndex(x => x.ProductId);

            // No back-navigation on ProductImage, which avoids JSON serialization cycles.
            // Deleting a product also deletes its image rows (the files are removed by ProductService).
            entity.HasOne<Product>()
                .WithMany(p => p.Images)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("Warehouses");
            entity.HasKey(x => x.WarehouseId);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.ToTable("Inventory");
            entity.HasKey(x => new { x.ProductId, x.WarehouseId });
            entity.HasOne(x => x.Product)
                .WithMany(x => x.InventoryItems)
                .HasForeignKey(x => x.ProductId);
            entity.HasOne(x => x.Warehouse)
                .WithMany(x => x.InventoryItems)
                .HasForeignKey(x => x.WarehouseId);
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.ToTable("Deliveries");
            entity.HasKey(x => x.DeliveryId);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.Deliveries)
                .HasForeignKey(x => x.ProductId);
            entity.HasOne(x => x.Warehouse)
                .WithMany(x => x.Deliveries)
                .HasForeignKey(x => x.WarehouseId);
        });
    }
}