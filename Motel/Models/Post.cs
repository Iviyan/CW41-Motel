using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using NpgsqlTypes;

namespace Motel.Models;

public enum Posts
{
    [Description("Администратор")] Admin = 1,
    [Description("Кадровик")] Hr = 2,
    [Description("Продавец")] Salesman = 3,
    [Description("Маркетолог")] MarketingSpecialist = 4,
    [Description("Горничная")] Maid = 5,
    [Description("Бухгалтер")] Accountant = 6,
}

public static class PostsExtensions
{
    public static readonly ValueDescription<Posts>[] Values = EnumHelper.Get<Posts>();

    public static ValueDescription<Posts> GetByName(string name) 
        => !TryGetByName(name, out var post) 
            ? throw new ArgumentException("Invalid name", nameof(name)) 
            : post;

    public static bool TryGetByName(string name, out ValueDescription<Posts> value)
    {
        value = default!;
        if (!Posts.TryParse(name, out Posts post)) return false;

        value = Values.First(p => p.Value == post);
        return true;
    }

    public static string GetName(this Posts post) => Values.First(p => p.Value == post).Description;
}

[Table("posts")]
public class Post
{
    [Column("id")] public int Id { get; set; }
    [Column("name")] public required string Name { get; set; }

    public virtual ICollection<Employee> Employees { get; } = null!;
}

public record PostCreateModel(string? Name);

public class PostCreateModelValidator : AbstractValidator<PostCreateModel>
{
    public PostCreateModelValidator()
    {
        RuleFor(post => post.Name).NotNull().MaximumLength(50);
    }
}