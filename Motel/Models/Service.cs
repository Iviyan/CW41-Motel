using System.Text.RegularExpressions;
using NpgsqlTypes;

namespace Motel.Models;

[Table("services")]
public class Service
{
    [Column("id")] public int Id { get; set; }
    [Column("name")] public required string Name { get; set; }
    [Column("price", TypeName = "money")] public double Price { get; set; }
    [Column("is_actual")] public bool IsActual { get; set; }
    
    [JsonIgnore] public virtual ICollection<ServiceOrder> ServiceOrders { get; } = null!;
}

public record ServiceCreateModel(
    string? Name,
    double? Price,
    bool? IsActual = true
);

public class ServiceCreateModelValidator : AbstractValidator<ServiceCreateModel>
{
    public ServiceCreateModelValidator()
    {
        RuleFor(service => service.Name).NotNull().MaximumLength(100);
        RuleFor(service => service.Price).NotNull().GreaterThanOrEqualTo(0);
        RuleFor(service => service.IsActual).NotNull();
    }
}

public class ServicePatchModel : PatchDtoBase
{
    private string? name;
    private double? price;
    private bool? isActual;
    
    public string? Name { get => name; set => SetField(ref name, value); }
    public double? Price { get => price; set => SetField(ref price, value); }
    public bool? IsActual { get => isActual; set => SetField(ref isActual, value); }
}

public class ServicePatchModelValidator : AbstractValidator<ServicePatchModel>
{
    public ServicePatchModelValidator()
    {
        RuleFor(service => service.Name).NotNull().MaximumLength(100).WhenPropertyChanged();
        RuleFor(service => service.Price).NotNull().GreaterThanOrEqualTo(0).WhenPropertyChanged();
        RuleFor(service => service.IsActual).NotNull().WhenPropertyChanged();
    }
}