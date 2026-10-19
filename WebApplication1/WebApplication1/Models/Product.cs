using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Product
{
    public int ProductId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 99999999)]
    public decimal Price { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
}
