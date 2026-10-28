namespace WebApplication1.Services.Contracts;

using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models;

public interface IDeliveriesService
{
    Task<IEnumerable<Delivery>> GetAllAsync();

    Task<Delivery?> GetByIdAsync(int id);

    Task CreateAsync(Delivery delivery);

    Task<bool> UpdateAsync(Delivery delivery);

    Task DeleteAsync(int id);

    Task<bool> ExistsAsync(int id);

    Task<SelectList> GetProductsSelectListAsync(
        int? selectedProductId = null);

    Task<SelectList> GetWarehousesSelectListAsync(
        int? selectedWarehouseId = null);
}
