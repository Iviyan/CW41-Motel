using System.Linq.Expressions;

namespace Motel.Controllers;

[ApiController]
public class AdvertisingContractsController : ControllerBase
{
    private readonly ILogger<AdvertisingContractsController> logger;

    public AdvertisingContractsController(ILogger<AdvertisingContractsController> logger)
    {
        this.logger = logger;
    }

    [HttpGet("/api/advertising-contracts"), Authorize(Policy = nameof(Posts.MarketingSpecialist))]
    public async Task<IActionResult> GetAll([FromServices] ApplicationContext context)
    {
        var result = await context.AdvertisingContracts
            .Include(c => c.Employee)
            .ThenInclude(r => r.User)
            .Select(c => new
            {
                c.Id,
                c.CompanyName,
                c.Datetime,
                c.Description,
                c.Cost,
                c.IsActive,
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
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("/api/advertising-contracts/{id:int}"), Authorize(Policy = nameof(Posts.MarketingSpecialist))]
    public async Task<IActionResult> Get(int id,
        [FromServices] ApplicationContext context)
    {
        var advertisingContract = await context.AdvertisingContracts.Where(e => e.Id == id)
            .Include(c => c.Employee)
            .ThenInclude(r => r.User)
            .Select(c => new
            {
                c.Id,
                c.CompanyName,
                c.Datetime,
                c.Description,
                c.Cost,
                c.IsActive,
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
            .FirstOrDefaultAsync();

        return advertisingContract != null ? Ok(advertisingContract) : NotFound();
    }

    [HttpPost("/api/advertising-contracts"), Authorize(Policy = nameof(Posts.MarketingSpecialist))]
    public async Task<IActionResult> Add([FromBody] AdvertisingContractCreateModel model,
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData)
    {
        int employeeId = requestData.UserId!.Value;

        AdvertisingContract advertisingContract = new()
        {
            CompanyName = model.CompanyName!,
            Datetime = model.Datetime!.Value,
            Description = model.Description!,
            Cost = model.Cost!.Value,
            EmployeeId = employeeId,
            IsActive = model.IsActive!.Value,
        };

        context.AdvertisingContracts.Add(advertisingContract);
        await context.SaveChangesAsync();

        var employee = await context.AdvertisingContracts.Entry(advertisingContract)
            .Reference(c => c.Employee).Query()
            .Select(e => new
            {
                e.Id, e.User.Login,
                e.FirstName, e.LastName, e.Patronymic,
                Post = e.PostId != null ? ((Posts)e.PostId).ToString("G") : null
            }).FirstAsync();

        return Ok(new
        {
            advertisingContract.Id,
            advertisingContract.CompanyName,
            advertisingContract.Datetime,
            advertisingContract.Description,
            advertisingContract.Cost,
            advertisingContract.IsActive,
            Employee = employee
        });
    }

    [HttpPatch("/api/advertising-contracts/{id:int}"), Authorize(Policy = nameof(Posts.MarketingSpecialist))]
    public async Task<IActionResult> Edit(int id,
        [FromServices] ApplicationContext context,
        [FromBody] AdvertisingContractPatchModel model)
    {
        if (!await context.AdvertisingContracts.AnyAsync(t => t.Id == id)) return NotFound();

        if (model.ChangedProperties.Count == 0) return StatusCode(StatusCodes.Status204NoContent);

        AdvertisingContract advertisingContract = new()
        {
            Id = id,
            CompanyName = model.CompanyName ?? "",
            Datetime = model.Datetime ?? DateTime.MinValue,
            Description = model.Description ?? "",
            Cost = model.Cost ?? 0,
            IsActive = model.IsActive ?? false,
        };
        context.AdvertisingContracts.AttachModified(advertisingContract, model.ChangedProperties);

        await context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpDelete("/api/advertising-contracts/{id:int}"), Authorize(Policy = nameof(Posts.MarketingSpecialist))]
    public async Task<IActionResult> Delete(int id,
        [FromServices] ApplicationContext context)
    {
        int c = await context.AdvertisingContracts.Where(e => e.Id == id).ExecuteDeleteAsync();

        return c == 1 ? StatusCode(StatusCodes.Status204NoContent) : NotFound();
    }
}