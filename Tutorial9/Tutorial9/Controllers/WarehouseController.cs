using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Tutorial9.Services;
using Tutorial9.Model;

namespace Tutorial9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IDbService _DbService;

    public WarehouseController(IDbService dbService)
    {
        _DbService = dbService;
    }
    
    
    [HttpPost("fulfillOrder")]
    public async Task<IActionResult> FulfillOrder([FromBody] Product_Warehouse request)
    {
        var result = await _DbService.FulfillOrder(request);
        return Ok(result);
    }
}