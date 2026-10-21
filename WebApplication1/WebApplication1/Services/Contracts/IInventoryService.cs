namespace WebApplication1.Services.Contracts;

using WebApplication1.Models;

public interface IInventoryService
{
    Task<IReadOnlyList<InventoryItem>> GetAllAsync(int? productId = null, int? warehouseId = null, CancellationToken ct = default);
    Task<InventoryItem?> GetByIdAsync(int productId, int warehouseId, CancellationToken ct = default);

    /// <exception cref="KeyNotFoundException">Product or warehouse does not exist.</exception>
    /// <exception cref="InvalidOperationException">Inventory record already exists.</exception>
    Task<InventoryItem> CreateAsync(InventoryItem item, CancellationToken ct = default);

    /// <summary>Sets the quantity to an absolute value. Returns null if not found.</summary>
    Task<InventoryItem?> UpdateQuantityAsync(int productId, int warehouseId, int quantity, CancellationToken ct = default);

    /// <summary>Adds (or subtracts) a delta. Returns null if not found.</summary>
    /// <exception cref="InvalidOperationException">Resulting quantity would be negative.</exception>
    Task<InventoryItem?> AdjustQuantityAsync(int productId, int warehouseId, int delta, CancellationToken ct = default);

    Task<bool> DeleteAsync(int productId, int warehouseId, CancellationToken ct = default);
}