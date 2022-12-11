using Microsoft.EntityFrameworkCore.Query;

namespace Motel.Controllers;

[ApiController]
public class RoomsController : ControllerBase
{
    private readonly ILogger<RoomsController> logger;

    public RoomsController(ILogger<RoomsController> logger)
    {
        this.logger = logger;
    }

    [HttpGet("/api/rooms"), Authorize(Roles = $"{nameof(Posts.Hr)},{nameof(Posts.Maid)},{nameof(Posts.Admin)}")]
    public async Task<IActionResult> GetAll(
        [FromServices] ApplicationContext context,
        [FromQuery] bool onlyFree = false
    )
    {
        DateTime now = DateTime.UtcNow;
        
        IQueryable<Room> query = context.Rooms.Include(r => r.RoomType).OrderBy(r => r.Number);
        if (onlyFree)
        {
            query = query.Where(r => r.IsReady &&
                !context.LeaseRooms.Where(lr => lr.LeaseAgreement.StartAt <= now && lr.LeaseAgreement.EndAt >= now)
                    .Select(lr => lr.RoomNumber).Contains(r.Number)
            );
        }

        var result = await query.ToListAsync();

        return Ok(result);
    }

    [HttpGet("/api/rooms/{id:int}"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Get(int id,
        [FromServices] ApplicationContext context)
    {
        var room = await context.Rooms.Where(e => e.Number == id).Include(r => r.RoomType).FirstOrDefaultAsync();

        return room != null ? Ok(room) : NotFound();
    }

    [HttpPost("/api/rooms"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Add([FromBody] RoomCreateModel model,
        [FromServices] ApplicationContext context)
    {
        if (await context.Rooms.AnyAsync(t => t.Number == model.Number))
            return Problem(title: "The room with this number already exists", statusCode: StatusCodes.Status400BadRequest);

        RoomType? roomType = await context.RoomTypes.FirstOrDefaultAsync(t => t.Id == model.RoomTypeId!.Value);

        if (roomType == null) return Problem(title: "Invalid room type", statusCode: StatusCodes.Status400BadRequest);

        Room room = new()
        {
            Number = model.Number!.Value,
            IsCleaningNeeded = model.IsCleaningNeeded!.Value,
            IsReady = model.IsReady!.Value,
            RoomType = roomType
        };

        context.Rooms.Add(room);
        await context.SaveChangesAsync();

        return Ok(room);
    }

    [HttpPatch("/api/rooms/{id:int}"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Edit(int id,
        [FromServices] ApplicationContext context,
        [FromBody] RoomPatchModel model)
    {
        if (!await context.Rooms.AnyAsync(t => t.Number == id)) return NotFound();

        if (model.ChangedProperties.Count == 0) return StatusCode(StatusCodes.Status204NoContent);

        await using var transaction = await context.Database.BeginTransactionAsync();

        bool isNumberChanged = model.IsFieldPresent(r => r.Number);

        if (isNumberChanged && model.Number!.Value != id)
        {
            if (await context.Rooms.AnyAsync(t => t.Number == model.Number))
                return Problem(title: "The room with this number already exists", statusCode: StatusCodes.Status400BadRequest);

            int c = await context.Rooms.Where(t => t.Number == id)
                .ExecuteUpdateAsync(p =>
                    p.SetProperty(t => t.Number, t => model.Number!.Value)
                );

            if (c == 0) return StatusCode(StatusCodes.Status409Conflict);
        }

        if (isNumberChanged)
            model.ChangedProperties.ExceptWith(new[] { nameof(RoomPatchModel.Number) });

        if (model.IsFieldPresent(r => r.RoomTypeId)
            && !await context.RoomTypes.AnyAsync(s => s.Id == model.RoomTypeId!.Value))
        {
            return Problem(title: "Invalid service id", statusCode: StatusCodes.Status400BadRequest);
        }

        Room room = new()
        {
            Number = isNumberChanged ? model.Number!.Value : id,
            IsCleaningNeeded = model.IsCleaningNeeded ?? false,
            IsReady = model.IsReady ?? false,
            RoomTypeId = model.RoomTypeId ?? 0
        };
        context.Rooms.AttachModified(room, model.ChangedProperties);

        await context.SaveChangesAsync();

        await transaction.CommitAsync();

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpDelete("/api/rooms/{id:int}"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Delete(int id,
        [FromServices] ApplicationContext context)
    {
        int c = await context.Rooms.Where(e => e.Number == id).ExecuteDeleteAsync();

        return c == 1 ? StatusCode(StatusCodes.Status204NoContent) : NotFound();
    }
}