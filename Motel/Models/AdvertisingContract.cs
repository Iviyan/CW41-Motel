using System.Numerics;
using System.Text.RegularExpressions;
using NpgsqlTypes;

namespace Motel.Models;

[Table("advertising_contracts")]
public class AdvertisingContract
{
    [Column("id")] public int Id { get; set; }
    [Column("company_name")] public required string CompanyName { get; set; }
    [Column("datetime")] public DateTime Datetime { get; set; }
    [Column("description")] public required string Description { get; set; }
    [Column("cost")] public double Cost { get; set; }
    [Column("employee_id")] public int EmployeeId { get; set; }
    [Column("is_active")] public bool IsActive { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}

public record AdvertisingContractCreateModel(
    string? CompanyName,
    DateTime? Datetime,
    string? Description,
    double? Cost,
    bool? IsActive = true
);

public class AdvertisingContractCreateModelValidator : AbstractValidator<AdvertisingContractCreateModel>
{
    public AdvertisingContractCreateModelValidator()
    {
        RuleFor(serviceOrder => serviceOrder.CompanyName).NotNull().MaximumLength(100);
        RuleFor(serviceOrder => serviceOrder.Datetime).NotNull();
        RuleFor(serviceOrder => serviceOrder.Description).NotNull();
        RuleFor(serviceOrder => serviceOrder.Cost).NotNull().GreaterThanOrEqualTo(0);
        RuleFor(serviceOrder => serviceOrder.IsActive).NotNull();
    }
}

public class AdvertisingContractPatchModel : PatchDtoBase
{
    private string? companyName;
    private DateTime? datetime;
    private string? description;
    private double? cost;
    private bool? isActive;

    public string? CompanyName { get => companyName; set => SetField(ref companyName, value); }
    public DateTime? Datetime { get => datetime; set => SetField(ref datetime, value); }
    public string? Description { get => description; set => SetField(ref description, value); }
    public double? Cost { get => cost; set => SetField(ref cost, value); }
    public bool? IsActive { get => isActive; set => SetField(ref isActive, value); }
}

public class AdvertisingContractPatchModelValidator : AbstractValidator<AdvertisingContractPatchModel>
{
    public AdvertisingContractPatchModelValidator()
    {
        RuleFor(serviceOrder => serviceOrder.CompanyName).NotNull().MaximumLength(100).WhenPropertyChanged();
        RuleFor(serviceOrder => serviceOrder.Datetime).NotNull().WhenPropertyChanged();
        RuleFor(serviceOrder => serviceOrder.Description).NotNull().WhenPropertyChanged();
        RuleFor(serviceOrder => serviceOrder.Cost).NotNull().GreaterThanOrEqualTo(0).WhenPropertyChanged();
        RuleFor(serviceOrder => serviceOrder.IsActive).NotNull().WhenPropertyChanged();
    }
}