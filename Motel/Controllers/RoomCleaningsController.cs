namespace Motel.Controllers;

[ApiController]
public class RoomCleaningsController : ControllerBase
{
    private readonly ILogger<RoomCleaningsController> logger;

    public RoomCleaningsController(ILogger<RoomCleaningsController> logger)
    {
        this.logger = logger;
    }

    [HttpGet("/api/room-cleanings"),
     Authorize(Roles = $"{nameof(Posts.Salesman)},{nameof(Posts.Maid)},{nameof(Posts.Admin)}")]
    public async Task<IActionResult> GetAll(
        [FromServices] ApplicationContext context,
        [FromQuery] int? roomNumber,
        [FromQuery] int? employeeId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool verbose = false
    )
    {
        IQueryable<RoomCleaning> query = context.RoomCleanings;

        if (roomNumber is { }) query = query.Where(c => c.RoomNumber == roomNumber);
        if (employeeId is { }) query = query.Where(c => c.EmployeeId == employeeId);
        if (from is { }) query = query.Where(c => c.Datetime >= from);
        if (to is { }) query = query.Where(c => c.Datetime <= to);

        if (verbose)
            return Ok(await query
                .Include(c => c.Room)
                .ThenInclude(r => r.RoomType)
                .Include(c => c.Employee)
                .ThenInclude(r => r.User)
                .Select(c => new
                {
                    c.RoomNumber, c.Datetime, c.EmployeeId,
                    c.Room,
                    Employee = new
                    {
                        c.Employee.Id,
                        c.Employee.User.Login,
                        c.Employee.FirstName,
                        c.Employee.LastName,
                        c.Employee.Patronymic,
                        Post = c.Employee.PostId != null ? ((Posts)c.Employee.PostId).ToString("G") : null
                    }
                })
                .ToListAsync());

        return Ok(await query
            .Select(c => new { c.RoomNumber, c.Datetime, c.EmployeeId })
            .ToListAsync()
        );
    }

    [HttpPost("/api/rooms/{roomNumber:int}/clean"), Authorize(Policy = nameof(Posts.Maid))]
    public async Task<IActionResult> NewRecord(int roomNumber,
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData)
    {
        if (!await context.Rooms.AnyAsync(r => r.Number == roomNumber))
            return Problem(title: "There is no room with this number", statusCode: StatusCodes.Status404NotFound);

        await using var transaction = await context.Database.BeginTransactionAsync();
        
        RoomCleaning roomCleaning = new()
        {
            RoomNumber = roomNumber,
            Datetime = DateTime.UtcNow.RoundToSeconds(),
            EmployeeId = requestData.UserId!.Value,
        };

        context.RoomCleanings.Add(roomCleaning);
        
        await context.SaveChangesAsync();

        await context.Rooms.Where(r => r.Number == roomNumber).ExecuteUpdateAsync(p =>
            p.SetProperty(r => r.IsCleaningNeeded, r => false));
        
        await transaction.CommitAsync();

        return Ok(new
        {
            roomCleaning.RoomNumber,
            roomCleaning.Datetime,
            roomCleaning.EmployeeId
        });
    }

    [HttpDelete("/api/room-cleanings/{roomNumber:int}/{datetime:datetime}"),
     Authorize(Roles = $"{nameof(Posts.Salesman)},{nameof(Posts.Maid)},{nameof(Posts.Admin)}")]
    public async Task<IActionResult> Delete(int roomNumber, DateTime dateTime,
        [FromServices] ApplicationContext context)
    {
        int c = await context.RoomCleanings.Where(e => e.RoomNumber == roomNumber && e.Datetime == dateTime).ExecuteDeleteAsync();

        return c > 0 ? StatusCode(StatusCodes.Status204NoContent) : NotFound();
    }
}