using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

public class ErrorController : Controller
{
    [Route("Error/{statusCode}")]
    public IActionResult HandleError(int statusCode)
    {
        Response.StatusCode = statusCode;

        if (statusCode == 404)
            return View("NotFound");

        if (statusCode == 500)
            return View("ServerError");

        return View("Error");
    }
}