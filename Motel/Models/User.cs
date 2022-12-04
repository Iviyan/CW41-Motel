using System.Diagnostics.CodeAnalysis;

namespace Motel.Models;

[Table("users")]
public class User
{
    [Column("id"), DatabaseGenerated(DatabaseGeneratedOption.None)] public int Id { get; set; }
    [Column("login")] public string Login { get; set; } = null!;
    [Column("password")] public string Password { get; set; } = null!;
    
    [ForeignKey(nameof(Id))] public virtual Employee Employee { get; set; } = null!;
    
    public virtual ICollection<RefreshToken> RefreshTokens { get; } = null!;
}

public record LoginModel(string? Login, string? Password);

public class LoginModelValidator : AbstractValidator<LoginModel>
{
    public LoginModelValidator()
    {
        RuleFor(user => user.Login).NotNull();
        RuleFor(user => user.Password).NotNull();
    }
}

public record ChangePasswordRequest(string? NewPassword, string? OldPassword);

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(user => user.NewPassword).NotNull().Length(1, 30);
        RuleFor(user => user.OldPassword).NotNull();
    }
}