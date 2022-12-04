namespace Motel.Controllers;

[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly ILogger<EmployeesController> logger;

    public EmployeesController(ILogger<EmployeesController> logger)
    {
        this.logger = logger;
    }

    [HttpGet("/api/employees"), Authorize(Policy = nameof(Posts.Hr))]
    public IActionResult GetAll([FromServices] ApplicationContext context)
    {
        var result = context.Employees.Select(e => new
        {
            e.Id, e.User.Login,
            e.FirstName,
            e.LastName,
            e.Patronymic,
            e.PassportSerial,
            e.PassportNumber,
            e.Birthday,
            e.Phone,
            Post = e.PostId != null ? ((Posts)e.PostId).ToString("G") : null
        });

        return Ok(result);
    }

    [HttpGet("/api/employees/{id:int}"), Authorize(Policy = nameof(Posts.Hr))]
    public async Task<IActionResult> Get(int id,
        [FromServices] ApplicationContext context)
    {
        var employee = await context.Employees
            .Where(e => e.Id == id)
            .Include(e => e.User)
            .Select(employee => new
            {
                Employee = employee,
                employee.User.Login
            })
            .FirstOrDefaultAsync();

        if (employee == null) return NotFound();

        var result = new
        {
            employee.Employee.Id, employee.Login,
            employee.Employee.FirstName,
            employee.Employee.LastName,
            employee.Employee.Patronymic,
            employee.Employee.PassportSerial,
            employee.Employee.PassportNumber,
            employee.Employee.Birthday,
            employee.Employee.Phone,
            Post = employee.Employee.PostId is { } postId ? ((Posts)postId).ToString("G") : null
        };

        return Ok(result);
    }

    [HttpPost("/api/employees"), Authorize(Policy = nameof(Posts.Hr))]
    public async Task<IActionResult> Add([FromBody] EmployeeCreateModel model,
        [FromServices] ApplicationContext context)
    {
        if (await context.Users.AnyAsync(x => x.Login == model.Login))
            return Problem(title: "The user with this login already exists", statusCode: StatusCodes.Status400BadRequest);

        Posts? post = null;
        if (model.Post is { } postName && Posts.TryParse(postName, out Posts parsedPost)) post = parsedPost;

        if (post == Posts.Admin)
            return Problem(title: "There can only be one admin", statusCode: StatusCodes.Status400BadRequest);

        Employee employee = new()
        {
            LastName = model.LastName!,
            FirstName = model.FirstName!,
            Patronymic = model.Patronymic,
            PassportSerial = model.PassportSerial!,
            PassportNumber = model.PassportNumber!,
            Birthday = model.Birthday!.Value,
            Phone = model.Phone!,
            PostId = (int?)post
        };

        User user = new()
        {
            Login = model.Login!,
            Password = AccountController.GetHashPassword(model.Password!),
            Employee = employee
        };
        context.Employees.Add(employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return Ok(new
        {
            user.Id, user.Login,
            user.Employee.FirstName,
            user.Employee.LastName,
            user.Employee.Patronymic,
            user.Employee.PassportSerial,
            user.Employee.PassportNumber,
            user.Employee.Birthday,
            user.Employee.Phone,
            model.Post,
        });
    }

    private static readonly string[] EmployeePatchModelUserProperties =
        { nameof(EmployeePatchModel.Login), nameof(EmployeePatchModel.Password) };

    [HttpPatch("/api/employees/{id:int}"), Authorize(Policy = nameof(Posts.Hr))]
    public async Task<IActionResult> Edit(int id,
        [FromServices] ApplicationContext context,
        [FromBody] EmployeePatchModel model)
    {
        // Each employee must have a user
        var employeeData = await context.Employees.Where(e => e.Id == id)
            .Select(e => new { Login = e.User.Login, Post = (Posts?)e.PostId })
            .FirstOrDefaultAsync();

        if (employeeData == null) return NotFound();

        bool loginChanged = model.IsFieldPresent(e => e.Login);
        bool passwordChanged = model.IsFieldPresent(e => e.Password);

        await using var transaction = await context.Database.BeginTransactionAsync();

        if (loginChanged || passwordChanged)
        {
            var user = new User { Id = id };
            var userEntry = context.Attach(user);

            if (loginChanged && employeeData.Login != model.Login)
            {
                if (await context.Users.AnyAsync(u => u.Login == model.Login))
                    return Problem(title: "The user with this login already exists", statusCode: StatusCodes.Status400BadRequest);

                user.Login = model.Login!;
                userEntry.Property(u => u.Login).IsModified = true;
            }

            if (passwordChanged)
            {
                user.Password = AccountController.GetHashPassword(model.Password!);
                userEntry.Property(u => u.Password).IsModified = true;

                await context.RefreshTokens
                    .Where(t => t.UserId == user.Id)
                    .ExecuteDeleteAsync();
            }
        }

        var changedEmployeeProperties = model.ChangedProperties.Except(EmployeePatchModelUserProperties).ToList();

        if (changedEmployeeProperties.Count > 0)
        {
            if (employeeData.Post == Posts.Admin && model.IsFieldPresent(m => m.Post))
                return Problem(title: "Unable to change administrator role", statusCode: StatusCodes.Status400BadRequest);

            Employee employee = new()
            {
                Id = id,
                LastName = model.LastName ?? "",
                FirstName = model.FirstName ?? "",
                Patronymic = model.Patronymic,
                PassportSerial = model.PassportSerial ?? "",
                PassportNumber = model.PassportNumber ?? "",
                Birthday = model.Birthday ?? new DateOnly(),
                Phone = model.Phone ?? "",
                PostId = model.Post is { } post ? (int)PostsExtensions.GetByName(post).Value : null
            };
            context.Employees.AttachModified(employee, changedEmployeeProperties,
                new[] { (nameof(EmployeePatchModel.Post), nameof(Employee.PostId)) });
        }

        await context.SaveChangesAsync();

        await transaction.CommitAsync();

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpDelete("/api/employees/{id:int}"), Authorize(Policy = nameof(Posts.Hr))]
    public async Task<IActionResult> Delete(int id,
        [FromServices] ApplicationContext context)
    {
        var employee = await context.Employees.Where(e => e.Id == id)
            .Select(e => new { Post = (Posts?)e.PostId }).FirstOrDefaultAsync();

        if (employee == null) return NotFound();

        if (employee.Post == Posts.Admin)
            return Problem(title: "Unable to remove administrator", statusCode: StatusCodes.Status400BadRequest);

        int c = await context.Employees.Where(e => e.Id == id).ExecuteDeleteAsync();

        return c == 1 ? StatusCode(StatusCodes.Status204NoContent) : StatusCode(StatusCodes.Status409Conflict);
    }
}