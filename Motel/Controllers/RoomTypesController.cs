namespace Motel.Controllers;

[ApiController]
public class RoomTypesController : ControllerBase
{
    private readonly ILogger<RoomTypesController> logger;

    public RoomTypesController(ILogger<RoomTypesController> logger)
    {
        this.logger = logger;
    }

    [HttpGet("/api/room-types"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> GetAll([FromServices] ApplicationContext context)
    {
        var result = await context.RoomTypes.ToListAsync();
        
        return Ok(result);
    }

    [HttpGet("/api/room-types/{id:int}"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Get(int id,
        [FromServices] ApplicationContext context)
    {
        var roomType = await context.RoomTypes.Where(e => e.Id == id).FirstOrDefaultAsync();
        
        return roomType != null ?  Ok(roomType) : NotFound();
    }

    [HttpPost("/api/room-types"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Add([FromBody] RoomTypeCreateModel model,
        [FromServices] ApplicationContext context)
    {
        if (await context.RoomTypes.AnyAsync(t => t.Name == model.Name))
            return Problem(title: "The room type with this name already exists", statusCode: StatusCodes.Status400BadRequest);
        
        RoomType roomType = new()
        {
            Name = model.Name!,
            PricePerHour = model.PricePerHour!.Value,
            Capacity = model.Capacity!.Value
        };
        
        context.RoomTypes.Add(roomType);
        await context.SaveChangesAsync();

        return Ok(roomType);
    }
    
    [HttpPatch("/api/room-types/{id:int}"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Edit(int id,
        [FromServices] ApplicationContext context,
        [FromBody] RoomTypePatchModel model)
    {
        if (!await context.RoomTypes.AnyAsync(t => t.Id == id)) return NotFound();

        if (model.ChangedProperties.Count == 0) return StatusCode(StatusCodes.Status204NoContent);
        
        if (model.IsFieldPresent(t => t.Name) 
            && await context.RoomTypes.AnyAsync(t => t.Name == model.Name && t.Id != id))
        {
            return Problem(title: "The room type with this name already exists", statusCode: StatusCodes.Status400BadRequest);
        }

        RoomType roomType = new()
        {
            Id = id,
            Name = model.Name ?? "",
            PricePerHour = model.PricePerHour ?? 0,
            Capacity = model.Capacity ?? 0
        };
        context.RoomTypes.AttachModified(roomType, model.ChangedProperties);

        await context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpDelete("/api/room-types/{id:int}"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Delete(int id,
        [FromServices] ApplicationContext context)
    {
        int c = await context.RoomTypes.Where(e => e.Id == id).ExecuteDeleteAsync();
        
        return c == 1 ? StatusCode(StatusCodes.Status204NoContent) : NotFound();
    }
}