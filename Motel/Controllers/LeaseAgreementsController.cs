namespace Motel.Controllers;

[ApiController]
public class LeaseAgreementsController : ControllerBase
{
    private readonly ILogger<LeaseAgreementsController> logger;

    public LeaseAgreementsController(ILogger<LeaseAgreementsController> logger)
    {
        this.logger = logger;
    }

    [HttpGet("/api/lease-agreements"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> GetAll([FromServices] ApplicationContext context)
    {
        var result = await context.LeaseAgreements
            .Select(l => new
            {
                l.Id, l.ClientName, l.StartAt, l.EndAt,
                Rooms = l.LeaseRooms.Select(lr => lr.RoomNumber)
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("/api/lease-agreements/{id:int}"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Get(int id,
        [FromServices] ApplicationContext context)
    {
        var leaseAgreement = await context.LeaseAgreements.Where(e => e.Id == id)
            .Select(l => new
            {
                l.Id, l.ClientName, l.StartAt, l.EndAt,
                Rooms = l.LeaseRooms.Select(lr => lr.RoomNumber)
            })
            .FirstOrDefaultAsync();

        return leaseAgreement != null ? Ok(leaseAgreement) : NotFound();
    }

    [HttpPost("/api/lease-agreements"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Add([FromBody] LeaseAgreementCreateModel model,
        [FromServices] ApplicationContext context)
    {
        var rooms = await context.Rooms
            .Where(r => model.Rooms!.Contains(r.Number))
            .Select(r => r.Number).ToListAsync();

        if (rooms.Count != model.Rooms!.Length)
        {
            var invalidRooms = model.Rooms!.Except(rooms).ToList();
            return Problem(title: $"Rooms with numbers [{String.Join(", ", invalidRooms)}] do not exist",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var startAt = model.StartAt!.Value;
        var endAt = model.EndAt!.Value;

        var occupiedRooms = await context.LeaseRooms.Include(l => l.LeaseAgreement)
            .Where(l => rooms.Contains(l.RoomNumber))
            .Where(l =>
                l.LeaseAgreement.StartAt <= startAt && l.LeaseAgreement.EndAt >= startAt
                || l.LeaseAgreement.StartAt <= endAt && l.LeaseAgreement.EndAt >= endAt
            )
            .Select(l => l.RoomNumber).ToListAsync();

        if (occupiedRooms.Count > 0)
        {
            return Problem(title: $"Rooms with numbers [{String.Join(", ", occupiedRooms)}] are occupied",
                statusCode: StatusCodes.Status400BadRequest);
        }

        LeaseAgreement leaseAgreement = new()
        {
            ClientName = model.ClientName!,
            StartAt = startAt,
            EndAt = endAt,
        };
        leaseAgreement.LeaseRooms = rooms.Select(roomNumber =>
            new LeaseRoom { RoomNumber = roomNumber, LeaseAgreement = leaseAgreement }).ToArray();

        context.LeaseAgreements.Add(leaseAgreement);
        await context.SaveChangesAsync();

        return Ok(new
        {
            leaseAgreement.Id, leaseAgreement.ClientName, leaseAgreement.StartAt, leaseAgreement.EndAt,
            Rooms = leaseAgreement.LeaseRooms.Select(lr => lr.RoomNumber)
        });
    }

    [HttpPatch("/api/lease-agreements/{id:int}"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Edit(int id,
        [FromServices] ApplicationContext context,
        [FromBody] LeaseAgreementPatchModel model)
    {
        if (!await context.LeaseAgreements.AnyAsync(t => t.Id == id)) return NotFound();

        if (model.ChangedProperties.Count == 0) return StatusCode(StatusCodes.Status204NoContent);

        // TODO: check
        bool isStartAtChanged = model.IsFieldPresent(l => l.StartAt);
        bool isEndAtChanged = model.IsFieldPresent(l => l.EndAt);
        if (isStartAtChanged || isEndAtChanged)
        {
            var startAt = model.StartAt;
            var endAt = model.EndAt;
            
            var query = context.LeaseRooms.Where(l =>
                context.LeaseAgreements.Where(ls => ls.Id == id).Select(ls => ls.Id).Contains(l.LeaseAgreementId));

            query = (isStartAtChanged, isEndAtChanged) switch
            {
                (true, true) => query.Where(l =>
                    l.LeaseAgreement.StartAt <= startAt && l.LeaseAgreement.EndAt >= startAt ||
                    l.LeaseAgreement.StartAt <= endAt && l.LeaseAgreement.EndAt >= endAt),
                (true, false) => query.Where(l => l.LeaseAgreement.StartAt <= startAt && l.LeaseAgreement.EndAt >= startAt),
                (false, true) => query.Where(l => l.LeaseAgreement.StartAt <= endAt && l.LeaseAgreement.EndAt >= endAt),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            var occupiedRooms = await query.Select(l => l.RoomNumber).ToListAsync();

            if (occupiedRooms.Count > 0)
            {
                return Problem(title: $"Rooms with numbers [{String.Join(", ", occupiedRooms)}] are occupied",
                    statusCode: StatusCodes.Status400BadRequest);
            }
        }

        LeaseAgreement leaseAgreement = new()
        {
            Id = id,
            ClientName = model.ClientName ?? "",
            StartAt = model.StartAt ?? DateTime.MinValue,
            EndAt = model.EndAt ?? DateTime.MinValue,
        };
        context.LeaseAgreements.AttachModified(leaseAgreement, model.ChangedProperties);

        await context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpDelete("/api/lease-agreements/{id:int}"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Delete(int id,
        [FromServices] ApplicationContext context)
    {
        int c = await context.LeaseAgreements.Where(e => e.Id == id).ExecuteDeleteAsync();

        return c == 1 ? StatusCode(StatusCodes.Status204NoContent) : NotFound();
    }
}