using Motel.Configuration;
using Motel.Controllers;

namespace Motel.Utils;

public static class InitDbUtils
{
    public static async Task Init(IServiceProvider provider, IConfiguration configuration)
    {
        using var scope = provider.CreateScope();

        var efContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        {
            var posts = await efContext.Posts.ToListAsync();
            var allPosts = PostsExtensions.Values.Select(v =>
                new Post { Id = (int)v.Value, Name = v.Value.ToString("G") }).ToArray();
            var postsToAdd = allPosts.ExceptBy(posts.Select(p => p.Id), p => p.Id).ToList();

            if (postsToAdd.Count > 0)
                logger.LogInformation("Posts ({PostList}) adding", String.Join(", ", postsToAdd.Select(p => p.Name)));

            efContext.Posts.AddRange(postsToAdd);
        }

        if (!await efContext.Users.AnyAsync(u => u.Employee.PostId == (int)Posts.Admin))
        {
            var firstUser = configuration.GetSection(AdminUserConfig.SectionName).Get<AdminUserConfig>();
            if (firstUser is { })
            {
                Employee employee = new()
                {
                    PostId = (int?)Posts.Admin, 
                    FirstName = "", LastName = "", PassportSerial = "0000", PassportNumber = "000000", Phone = ""
                };
                User user = new()
                {
                    Login = firstUser.Login,
                    Password = AccountController.GetHashPassword(firstUser.Password),
                    Employee = employee
                };
                efContext.Employees.Add(employee);
                efContext.Users.Add(user);

                logger.LogInformation("Admin user creating");
            }
            else
            {
                logger.LogWarning("There are admin user in the database, but the one's configuration was not set");
            }
        }

        await efContext.SaveChangesAsync();
    }
}