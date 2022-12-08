global using System.Text;
global using System.Data;
global using System.Net.Mime;
global using System.Text.Json;
global using System.Security.Claims;
global using System.IdentityModel.Tokens.Jwt;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Options;
global using Microsoft.AspNetCore.SpaServices;
global using Microsoft.AspNetCore.SignalR;
global using Microsoft.EntityFrameworkCore;
global using System.Text.Json.Serialization;
global using System.ComponentModel;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using System.Globalization;
global using VueCliMiddleware;
global using Microsoft.AspNetCore.Localization;
global using Npgsql;
global using CsvHelper;
global using FluentValidation;
global using FluentValidation.AspNetCore;
global using Motel.Database;
global using Motel.Models;
global using Motel.Utils;
using System.Diagnostics;
using Motel.Configuration;
using Motel;
using Motel.Controllers;
using Motel.Services;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

configuration
    .AddJsonFile("secrets.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"secrets.{builder.Environment.EnvironmentName}.json",
        optional: true, reloadOnChange: true);

// Database configuration

string connectionString = builder.Configuration.GetConnectionString("PgsqlConnection")!;

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
var dataSource = dataSourceBuilder.Build();

services.AddDbContextPool<ApplicationContext>(options =>
{
    options.UseNpgsql(dataSource);
#if DEBUG
    options.LogTo(m => Debug.WriteLine(m), LogLevel.Trace)
        .EnableSensitiveDataLogging();
#endif
}, poolSize: 16); // Error with NpgsqlConnection in ClearOldRefreshTokensService

/*services.AddDbContext<ApplicationContext>(options =>
{
    options.UseNpgsql(dataSource);
#if DEBUG
    options.LogTo(m => Debug.WriteLine(m), LogLevel.Trace)
        .EnableSensitiveDataLogging();
#endif
});*/

services.AddSingleton(dataSource);

// FluentValidation configuration (part)

ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en");
ValidatorOptions.Global.DisplayNameResolver = (type, member, expression) => CamelCaseNamingPolicy.ToCamelCase(member.Name);

// WhenPropertyChanged not work with this, TODO: fix
ValidatorOptions.Global.PropertyNameResolver = (type, member, expression) => CamelCaseNamingPolicy.ToCamelCase(member.Name);
        
ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

// Controllers, JSON, FluentValidation configuration

services.AddControllers(options =>
    {
        options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider());
        options.ValueProviderFactories.Add(new SnakeCaseQueryValueProviderFactory());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = CamelCaseNamingPolicy.Instance;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(CamelCaseNamingPolicy.Instance));
#if !DEBUG
        options.AllowInputFormatterExceptionMessages = false;
#endif
        options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
    });

services.AddFluentValidationAutoValidation(fv => { fv.DisableDataAnnotationsValidation = true; });
services.AddValidatorsFromAssemblyContaining<Program>(lifetime: ServiceLifetime.Singleton);

// JWT configuration

services.Configure<JwtConfig>(configuration.GetSection(JwtConfig.SectionName));

var jwtConfig = configuration.GetSection(JwtConfig.SectionName).Get<JwtConfig>();

var tokenValidationParams = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),
    ValidIssuer = jwtConfig.Issuer,
    ValidateIssuer = true,
    ValidateAudience = false,
    ValidateLifetime = true,
    RequireExpirationTime = false,
    ClockSkew = TimeSpan.Zero,
    RoleClaimType = "role",
};

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Authentication with JWT configuration

services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(jwt =>
    {
        jwt.SaveToken = true;
        jwt.TokenValidationParameters = tokenValidationParams;
    });

services.AddAuthorization(options =>
{
    options.AddPolicy(nameof(Posts.Hr),
        policy => policy.RequireRole(nameof(Posts.Hr), nameof(Posts.Admin)));
    options.AddPolicy(nameof(Posts.Accountant),
        policy => policy.RequireRole(nameof(Posts.Accountant), nameof(Posts.Admin)));
    options.AddPolicy(nameof(Posts.Salesman),
        policy => policy.RequireRole(nameof(Posts.Salesman), nameof(Posts.Admin)));
    options.AddPolicy(nameof(Posts.MarketingSpecialist),
        policy => policy.RequireRole(nameof(Posts.MarketingSpecialist), nameof(Posts.Admin)));
    options.AddPolicy(nameof(Posts.Maid),
        policy => policy.RequireRole(nameof(Posts.Maid), nameof(Posts.Admin)));
});

services.AddScoped<RequestData>(); // Information about the user who made the request

// SPA configuration
services.AddSpaStaticFiles(options => options.RootPath = "ClientApp/dist");

// A service that removes obsolete refresh tokens
services.AddHostedService<ClearOldRefreshTokensService>();

var app = builder.Build();

Console.WriteLine("Current culture: " + CultureInfo.CurrentCulture);

// Init posts (roles)
// Creating a first (admin) user.
await InitDbUtils.Init(app.Services, configuration);

if (app.Environment.IsDevelopment()) { }

app.UseSpaStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Get device uid, user id, user login for every request. Sets the device uid if it doesn't exist.
app.Use(async (context, next) =>
{
    RequestData requestData = context.RequestServices.GetRequiredService<RequestData>();
    if (!context.Request.Cookies.TryGetValue("DeviceUid", out string? deviceSUid)
        || !Guid.TryParse(deviceSUid, out Guid deviceUid))
    {
        requestData.DeviceUid = Guid.NewGuid();
        context.Response.Cookies.Append("DeviceUid", requestData.DeviceUid.ToString(),
            new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.FromUnixTimeSeconds(int.MaxValue) });
    }
    else
        requestData.DeviceUid = deviceUid;

    if (context.User.Identity?.IsAuthenticated is true)
    {
        if (context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value is { } userId)
            requestData.UserId = int.Parse(userId);
        if (context.User.FindFirst(JwtRegisteredClaimNames.Name)?.Value is { } login)
            requestData.UserLogin = login;
        if (context.User.FindFirst(ClaimTypes.Role)?.Value is { } postName
            && PostsExtensions.TryGetByName(postName, out var post))
            requestData.Post = post.Value;
    }

    await next();
});

app.MapControllers();

// https://github.com/dotnet/aspnetcore/issues/5223#issuecomment-433394061
//if (false)
{
    var spaApp = ((IEndpointRouteBuilder)app).CreateApplicationBuilder();
    spaApp.Use(next => context =>
    {
        // Set endpoint to null so the SPA middleware will handle the request.
        context.SetEndpoint(null);
        return next(context);
    });

    spaApp.UseFixedSpa(spaBuilder =>
    {
        spaBuilder.Options.SourcePath = "ClientApp";

        if (System.Diagnostics.Debugger.IsAttached)
        {
            spaBuilder.UseVueCli(
                npmScript: "serve",
                port: 8080,
                https: false,
                runner: ScriptRunnerType.Npm,
                regex: "Compiled successfully",
                forceKill: true,
                wsl: false);
        }
    });

    app.MapFallback("{*path}", spaApp.Build()); // default is {*path:nonfile}
}

app.Run();