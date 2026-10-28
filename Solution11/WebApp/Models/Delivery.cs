using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Delivery
{
    public int DeliveryId { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [DataType(DataType.Date)]
    public DateTime DeliveryDate { get; set; } = DateTime.Today;

    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
}
