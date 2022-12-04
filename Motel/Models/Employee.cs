using System.Linq.Expressions;
using System.Text.RegularExpressions;
using NpgsqlTypes;

namespace Motel.Models;

[Table("employees")]
public class Employee
{
    [Column("id")] public int Id { get; set; }
    [Column("last_name")] public required string LastName { get; set; }
    [Column("first_name")] public required string FirstName { get; set; }
    [Column("patronymic")] public string? Patronymic { get; set; } = null!;
    [Column("passport_serial")] public required string PassportSerial { get; set; }
    [Column("passport_number")] public required string PassportNumber { get; set; }
    [Column("birthday")] public DateOnly Birthday { get; set; }
    [Column("phone")] public required string Phone { get; set; }
    [Column("post_id")] public int? PostId { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Post? Post { get; set; } = null!;

    public virtual ICollection<AdvertisingContract> AdvertisingContracts { get; } = null!;
    public virtual ICollection<RoomCleaning> RoomCleanings { get; } = null!;
}

public record EmployeeCreateModel(
    // Employee
    string? LastName,
    string? FirstName,
    string? Patronymic,
    string? PassportSerial,
    string? PassportNumber,
    DateOnly? Birthday,
    string? Phone,
    string? Post,
    // User
    string? Login,
    string? Password
);

public static class EmployeeValidatorUtils
{
    public static readonly Regex NamePartRegex =
        new(@"^[а-яёa-z]*$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    public static readonly Regex PhoneRegex =
        new(@"^\+\d{1,3}\d{3,5}\d{7}$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    public static readonly string NamePartRegexError = "'{PropertyName}' must contain only alphabetic characters";
    public static readonly string PassportPartRegexError = "'{PropertyName}' must contain only numbers";
    public static readonly string PostParseError = "Invalid post";
}

public class EmployeeCreateModelValidator : AbstractValidator<EmployeeCreateModel>
{
    public EmployeeCreateModelValidator()
    {
        RuleFor(employee => employee.LastName).NotNull().NotEmpty().MaximumLength(30)
            .Matches(EmployeeValidatorUtils.NamePartRegex).WithMessage(EmployeeValidatorUtils.NamePartRegexError);
        RuleFor(employee => employee.FirstName).NotNull().NotEmpty().MaximumLength(30)
            .Matches(EmployeeValidatorUtils.NamePartRegex).WithMessage(EmployeeValidatorUtils.NamePartRegexError);
        RuleFor(employee => employee.Patronymic).MaximumLength(30)
            .Matches(EmployeeValidatorUtils.NamePartRegex).WithMessage(EmployeeValidatorUtils.NamePartRegexError);
        RuleFor(employee => employee.PassportSerial).NotNull().Length(4)
            .Must(c => c.All(Char.IsDigit)).WithMessage(EmployeeValidatorUtils.PassportPartRegexError);
        RuleFor(employee => employee.PassportNumber).NotNull().Length(6)
            .Must(c => c.All(Char.IsDigit)).WithMessage(EmployeeValidatorUtils.PassportPartRegexError);
        RuleFor(employee => employee.Birthday).NotNull().LessThan(DateOnly.FromDateTime(DateTime.Now));
        RuleFor(employee => employee.Phone).NotNull().MaximumLength(20).Matches(EmployeeValidatorUtils.PhoneRegex);
        RuleFor(employee => employee.Post)
            .Must(p => p is null || PostsExtensions.TryGetByName(p, out _)).WithMessage(EmployeeValidatorUtils.PostParseError);

        RuleFor(user => user.Login).NotNull().Length(0, 30);
        RuleFor(user => user.Password).NotNull().Length(1, 30);
    }
}

public class EmployeePatchModel : PatchDtoBase
{
    private string? lastName;
    private string? firstName;
    private string? patronymic;
    private string? passportSerial;
    private string? passportNumber;
    private DateOnly? birthday;
    private string? phone;
    private string? post;
    private string? login;
    private string? password;

    public string? LastName { get => lastName; set => SetField(ref lastName, value); }
    public string? FirstName { get => firstName; set => SetField(ref firstName, value); }
    public string? Patronymic { get => patronymic; set => SetField(ref patronymic, value); }
    public string? PassportSerial { get => passportSerial; set => SetField(ref passportSerial, value); }
    public string? PassportNumber { get => passportNumber; set => SetField(ref passportNumber, value); }
    public DateOnly? Birthday { get => birthday; set => SetField(ref birthday, value); }
    public string? Phone { get => phone; set => SetField(ref phone, value); }
    public string? Post { get => post; set => SetField(ref post, value); }
    public string? Login { get => login; set => SetField(ref login, value); }
    public string? Password { get => password; set => SetField(ref password, value); }
}

/* set => (\w+) = value;
   set => SetField(ref $1, value); */

public class EmployeePatchModelValidator : AbstractValidator<EmployeePatchModel>
{
    public EmployeePatchModelValidator()
    {
        RuleFor(employee => employee.LastName).NotNull().MaximumLength(30)
            .Matches(EmployeeValidatorUtils.NamePartRegex).WithMessage(EmployeeValidatorUtils.NamePartRegexError)
            //.When(p => p.IsFieldPresent(m => m.LastName));
            .WhenPropertyChanged();
        RuleFor(employee => employee.FirstName).NotNull().MaximumLength(30)
            .Matches(EmployeeValidatorUtils.NamePartRegex).WithMessage(EmployeeValidatorUtils.NamePartRegexError).WhenPropertyChanged();
        RuleFor(employee => employee.Patronymic).MaximumLength(30)
            .Matches(EmployeeValidatorUtils.NamePartRegex).WithMessage(EmployeeValidatorUtils.NamePartRegexError).WhenPropertyChanged();
        RuleFor(employee => employee.PassportSerial).NotNull().Length(4)
            .Must(c => c.All(Char.IsDigit)).WithMessage(EmployeeValidatorUtils.PassportPartRegexError).WhenPropertyChanged();
        RuleFor(employee => employee.PassportNumber).NotNull().Length(6)
            .Must(c => c.All(Char.IsDigit)).WithMessage(EmployeeValidatorUtils.PassportPartRegexError).WhenPropertyChanged();
        RuleFor(employee => employee.Birthday).NotNull().LessThan(DateOnly.FromDateTime(DateTime.Now)).WhenPropertyChanged();
        RuleFor(employee => employee.Phone).NotNull().MaximumLength(20).Matches(EmployeeValidatorUtils.PhoneRegex).WhenPropertyChanged();
        RuleFor(employee => employee.Post)
            .Must(p => p is null || PostsExtensions.TryGetByName(p, out _)).WithMessage(EmployeeValidatorUtils.PostParseError)
            .WhenPropertyChanged();

        RuleFor(user => user.Login).NotNull().Length(0, 30).WhenPropertyChanged();
        RuleFor(user => user.Password).NotNull().Length(1, 30).WhenPropertyChanged();
    }
}