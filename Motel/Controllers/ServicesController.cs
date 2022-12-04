namespace Motel.Controllers;

[ApiController]
public class ServicesController : ControllerBase
{
    private readonly ILogger<ServicesController> logger;

    public ServicesController(ILogger<ServicesController> logger)
    {
        this.logger = logger;
    }

    [HttpGet("/api/services"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> GetAll(
        [FromServices] ApplicationContext context,
        [FromQuery] bool includeNonActual = true)
    {
        IQueryable<Service> query = context.Services;

        if (!includeNonActual) query = query.Where(s => s.IsActual == true);

        var result = await query.ToListAsync();
        
        return Ok(result);
    }

    [HttpGet("/api/services/{id:int}"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Get(int id,
        [FromServices] ApplicationContext context)
    {
        var service = await context.Services.Where(e => e.Id == id).FirstOrDefaultAsync();
        
        return service != null ?  Ok(service) : NotFound();
    }

    [HttpPost("/api/services"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Add([FromBody] ServiceCreateModel model,
        [FromServices] ApplicationContext context)
    {
        if (await context.Services.AnyAsync(t => t.Name == model.Name))
            return Problem(title: "The service with this name already exists", statusCode: StatusCodes.Status400BadRequest);
        
        Service service = new()
        {
            Name = model.Name!,
            Price = model.Price!.Value,
            IsActual = model.IsActual!.Value
        };
        
        context.Services.Add(service);
        await context.SaveChangesAsync();

        return Ok(service);
    }
    
    [HttpPatch("/api/services/{id:int}"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Edit(int id,
        [FromServices] ApplicationContext context,
        [FromBody] ServicePatchModel model)
    {
        if (!await context.Services.AnyAsync(t => t.Id == id)) return NotFound();

        if (model.ChangedProperties.Count == 0) return StatusCode(StatusCodes.Status204NoContent);
        
        if (model.IsFieldPresent(t => t.Name) 
            && await context.Services.AnyAsync(t => t.Name == model.Name && t.Id != id))
        {
            return Problem(title: "The service with this name already exists", statusCode: StatusCodes.Status400BadRequest);
        }

        Service service = new()
        {
            Id = id,
            Name = model.Name ?? "",
            Price = model.Price ?? 0,
            IsActual = model.IsActual ?? false
        };
        context.Services.AttachModified(service, model.ChangedProperties);

        await context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpDelete("/api/services/{id:int}"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Delete(int id,
        [FromServices] ApplicationContext context)
    {
        int c = await context.Services.Where(e => e.Id == id).ExecuteDeleteAsync();
        
        return c == 1 ? StatusCode(StatusCodes.Status204NoContent) : NotFound();
    }
}