using System.ComponentModel.DataAnnotations;

namespace WarehouseInventoryMvc.Models;

public class InventoryItem
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
}
