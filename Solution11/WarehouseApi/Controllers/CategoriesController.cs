using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

namespace WarehouseApi.Controllers;

[ApiController]
[Authorize]
[Route("api/categories")]
public sealed class CategoriesController(ICategoriesService categories) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetAll() =>
        Ok(await categories.GetAllAsync());
}
