using System.Diagnostics.Metrics;
using CsvHelper.Configuration;

namespace Motel.Controllers;

[ApiController]
public class ServiceOrdersController : ControllerBase
{
    private readonly ILogger<ServiceOrdersController> logger;

    public ServiceOrdersController(ILogger<ServiceOrdersController> logger)
    {
        this.logger = logger;
    }

    [HttpGet("/api/service-orders"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> GetAll(
        [FromServices] ApplicationContext context,
        [FromQuery] int? roomNumber,
        [FromQuery] int? serviceId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool verbose = false
    )
    {
        IQueryable<ServiceOrder> query = context.ServiceOrders;

        if (roomNumber is { }) query = query.Where(o => o.RoomNumber == roomNumber);
        if (serviceId is { }) query = query.Where(o => o.ServiceId == serviceId);
        if (from is { }) query = query.Where(o => o.Datetime >= from);
        if (to is { }) query = query.Where(o => o.Datetime <= to);

        if (verbose)
            return Ok(await query.AsNoTracking()
                .Include(o => o.Room)
                .ThenInclude(r => r.RoomType)
                .Include(o => o.Service)
                .ToListAsync());

        return Ok(await query
            .Select(o => new { o.Id, o.RoomNumber, o.ServiceId })
            .ToListAsync()
        );
    }

    [HttpGet("/api/service-orders/{id:int}"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Get(int id,
        [FromServices] ApplicationContext context)
    {
        var result = await context.ServiceOrders.AsNoTracking()
            .Where(o => o.Id == id)
            .Include(o => o.Room)
            .ThenInclude(r => r.RoomType)
            .Include(o => o.Service)
            .FirstOrDefaultAsync();

        return Ok(result);
    }

    [HttpPost("/api/service-orders"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> NewOrder(
        [FromBody] ServiceOrderCreateModel model,
        [FromServices] ApplicationContext context)
    {
        if (!await context.Services.AnyAsync(s => s.Id == model.ServiceId!.Value))
            return Problem(title: "Invalid service id", statusCode: StatusCodes.Status400BadRequest);

        if (!await context.Rooms.AnyAsync(r => r.Number == model.RoomNumber!.Value))
            return Problem(title: "Invalid room number", statusCode: StatusCodes.Status400BadRequest);

        ServiceOrder serviceOrder = new()
        {
            RoomNumber = model.RoomNumber!.Value,
            ServiceId = model.ServiceId!.Value,
            Datetime = model.Datetime!.Value,
        };

        context.ServiceOrders.Add(serviceOrder);
        await context.SaveChangesAsync();

        return Ok(new
        {
            serviceOrder.Id,
            serviceOrder.RoomNumber,
            serviceOrder.ServiceId,
            serviceOrder.Datetime
        });
    }

    [HttpDelete("/api/service-orders/{id:int}"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> Delete(int id,
        [FromServices] ApplicationContext context)
    {
        int c = await context.ServiceOrders.Where(e => e.Id == id).ExecuteDeleteAsync();

        return c == 1 ? StatusCode(StatusCodes.Status204NoContent) : NotFound();
    }


    [HttpGet("/api/service-orders/csv"), Authorize(Policy = nameof(Posts.Salesman))]
    public async Task<IActionResult> GetCsv(
        [FromServices] ApplicationContext context,
        [FromQuery] int? roomNumber,
        [FromQuery] int? serviceId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to
    )
    {
        IQueryable<ServiceOrder> query = context.ServiceOrders;

        if (roomNumber is { }) query = query.Where(o => o.RoomNumber == roomNumber);
        if (serviceId is { }) query = query.Where(o => o.ServiceId == serviceId);
        if (from is { }) query = query.Where(o => o.Datetime >= from);
        if (to is { }) query = query.Where(o => o.Datetime <= to);

        var serviceOrders = await query.AsNoTracking()
            .Include(o => o.Room)
            .ThenInclude(r => r.RoomType)
            .Include(o => o.Service)
            .Select(o => new ServiceOrderCsv(
                o.Id, o.Datetime,
                o.Service.Name, o.Service.Price,
                o.RoomNumber, o.Room.RoomType.Name
            ))
            .ToListAsync();

        MemoryStream stream = new();
        using var writer = new StreamWriter(stream);
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
               {
                   Delimiter = ";" // excel
               }))
        {
            csv.Context.RegisterClassMap<ServiceOrderCsvMap>();
            csv.WriteRecords(serviceOrders);
        }

        return File(stream.ToArray(), "text/csv",
            $"Service orders [{from:dd.MM.yyyy HH.mm.ss} - {to:dd.MM.yyyy HH.mm.ss}].csv");
    }

    record ServiceOrderCsv(
        int Id, DateTime Datetime,
        string ServiceName, double ServicePrice,
        int RoomNumber, string RoomType);

    sealed class ServiceOrderCsvMap : ClassMap<ServiceOrderCsv>
    {
        public ServiceOrderCsvMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.Datetime)
                .Convert(m => m.Value.Datetime.ToLocalTime().ToString(CultureInfo.CurrentCulture));
        }
    }
}