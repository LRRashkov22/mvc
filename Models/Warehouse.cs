using System.ComponentModel.DataAnnotations;

namespace WarehouseInventoryMvc.Models;

public class Warehouse
{
    public int WarehouseId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
}
