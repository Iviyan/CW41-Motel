using System.Numerics;
using Microsoft.AspNetCore.Identity;
using Motel.Configuration;

namespace Motel.Controllers;

[ApiController]
public class AccountController : ControllerBase
{
    private static readonly PasswordHasher<User> PasswordHasher = new();

    private readonly ILogger<AccountController> logger;
    private readonly JwtConfig jwtConfig;

    public AccountController(ILogger<AccountController> logger, IOptions<JwtConfig> jwtConfig)
    {
        this.logger = logger;
        this.jwtConfig = jwtConfig.Value;
    }

    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model,
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData)
    {
        User? user = await context.Users
            .Include(u => u.Employee)
            .Select(u => new User
            {
                Id = u.Id, Login = u.Login, Password = u.Password,
                Employee = new Employee
                {
                    FirstName = u.Employee.FirstName,
                    LastName = u.Employee.LastName,
                    Patronymic = u.Employee.Patronymic,
                    PostId = u.Employee.PostId,
                    PassportSerial = null!, PassportNumber = null!, Phone = null!
                }
            })
            .FirstOrDefaultAsync(x => x.Login == model.Login);

        if (user == null)
            return Problem(title: "Invalid login", statusCode: StatusCodes.Status401Unauthorized);

        var passwordVerificationResult = PasswordHasher.VerifyHashedPassword(null!, user.Password, model.Password!);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
            return Problem(title: "Invalid password", statusCode: StatusCodes.Status401Unauthorized);

        string jwt = CreateJwtToken(user.Login, user.Id, (Posts?)user.Employee.PostId);

        // delete old refresh token
        await context.RefreshTokens.Where(t => t.DeviceUid == requestData.DeviceUid).ExecuteDeleteAsync();

        RefreshToken refreshToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Expires = DateTime.UtcNow + TimeSpan.FromDays(30),
            DeviceUid = requestData.DeviceUid
        };
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        var response = new
        {
            AccessToken = jwt,
            User = new
            {
                user.Id, user.Login,
                user.Employee.FirstName,
                user.Employee.LastName,
                user.Employee.Patronymic,
                Post = user.Employee.PostId is int postId ? PostsExtensions.Values[postId].Description : null
            }
        };

        Response.Cookies.Append("RefreshToken", refreshToken.Id.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                // Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.Now.Add(TimeSpan.FromDays(30))
            });

        return Ok(response);
    }

    public static string GetHashPassword(string password) => PasswordHasher.HashPassword(null!, password);

    [HttpPost("/refresh-token")]
    public async Task<IActionResult> RefreshToken(
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData)
    {
        if (!Request.Cookies.TryGetValue("RefreshToken", out string? sToken)
            || !Guid.TryParse(sToken, out Guid token))
            return Problem(title: "There is no RefreshToken cookie", statusCode: StatusCodes.Status400BadRequest);

        RefreshToken? refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(t => t.Id == token);

        if (refreshToken == null || refreshToken.Expires <= DateTime.UtcNow)
            return Problem(title: "Invalid or expired token", statusCode: StatusCodes.Status400BadRequest);

        if (refreshToken.DeviceUid != requestData.DeviceUid)
            return Problem(title: "Invalid token", detail: "The token was created by another client",
                statusCode: StatusCodes.Status400BadRequest);

        var user = await context.Users
            .Select(u => new { u.Id, u.Login, u.Employee.PostId })
            .FirstOrDefaultAsync(u => u.Id == refreshToken.UserId);
        if (user == null) return Problem(title: "The user no longer exists", statusCode: StatusCodes.Status400BadRequest);
        string jwt = CreateJwtToken(user.Login, user.Id, (Posts?)user.PostId);

        RefreshToken newRefreshToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Expires = DateTime.UtcNow + TimeSpan.FromDays(30),
            DeviceUid = requestData.DeviceUid
        };
        context.RefreshTokens.Remove(refreshToken);
        context.RefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync();

        var response = new { AccessToken = jwt };

        Response.Cookies.Append("RefreshToken", newRefreshToken.Id.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                // Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.Now.Add(TimeSpan.FromDays(30))
            });

        return Ok(response);
    }

    [HttpPost("/logout"), Authorize]
    public async Task<IActionResult> Logout(
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData)
    {
        if (!Request.Cookies.TryGetValue("RefreshToken", out string? sToken)
            || !Guid.TryParse(sToken, out Guid token))
            return Problem(title: "There is no RefreshToken cookie", statusCode: StatusCodes.Status400BadRequest);

        RefreshToken? refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(t => t.Id == token);
        if (refreshToken == null)
            return Problem(title: "Invalid or expired token", statusCode: StatusCodes.Status400BadRequest);
        if (refreshToken.DeviceUid != requestData.DeviceUid)
            return Problem(title: "Invalid token", detail: "The token was created on another client",
                statusCode: StatusCodes.Status400BadRequest);

        context.RefreshTokens.Remove(refreshToken);
        await context.SaveChangesAsync();

        Response.Cookies.Delete("RefreshToken");

        return Ok();
    }

    string CreateJwtToken(string login, int userId, Posts? post)
    {
        var now = DateTime.UtcNow;

        var jwt = new JwtSecurityToken(
            issuer: jwtConfig.Issuer,
            notBefore: now,
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Name, login),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim("role", post is { } p ? p.ToString("G") : string.Empty)
            },
            expires: now.Add(TimeSpan.FromHours(2)),
            signingCredentials: new SigningCredentials(jwtConfig.SecretKey,
                SecurityAlgorithms.HmacSha256));

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        return encodedJwt;
    }

    [HttpPost("/change-password"), Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData,
        [FromBody] ChangePasswordRequest model)
    {
        if (!Request.Cookies.TryGetValue("RefreshToken", out string? sRefreshToken)
            || !Guid.TryParse(sRefreshToken, out Guid refreshToken))
            return Problem(title: "There is no RefreshToken cookie", statusCode: StatusCodes.Status400BadRequest);

        string? password = await context.Users
            .Where(u => u.Id == requestData.UserId)
            .Select(u => u.Password)
            .FirstOrDefaultAsync();

        if (password is null)
            return Problem(title: "User not found", statusCode: StatusCodes.Status404NotFound);

        if (PasswordHasher.VerifyHashedPassword(null!, password, model.NewPassword!)
            != PasswordVerificationResult.Failed)
            return Problem(title: "The old password does not match the current one",
                statusCode: StatusCodes.Status400BadRequest);

        await using var transaction = await context.Database.BeginTransactionAsync();

        int c = await context.Users
            .Where(u => u.Id == requestData.UserId)
            .ExecuteUpdateAsync(u =>
                u.SetProperty(p => p.Password, p => GetHashPassword(model.NewPassword!)));

        await context.RefreshTokens
            .Where(t => t.UserId == requestData.UserId && t.Id != refreshToken)
            .ExecuteDeleteAsync();

        await transaction.CommitAsync();

        return c > 0
            ? StatusCode(StatusCodes.Status204NoContent)
            : Problem(title: "Unknown error", statusCode: StatusCodes.Status404NotFound);
    }
    
    [HttpGet("/api/me"), Authorize]
    public async Task<IActionResult> GetAll(
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData
        )
    {
        var result = await context.Employees.Select(e => new
        {
            e.Id, e.User.Login,
            e.FirstName, e.LastName, e.Patronymic,
            Post = e.PostId != null ? ((Posts)e.PostId).ToString("G") : null
        }).FirstAsync(e => e.Id == requestData.UserId);

        return Ok(result);
    }
}