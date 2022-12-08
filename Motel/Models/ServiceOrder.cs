using System.Text.RegularExpressions;
using NpgsqlTypes;

namespace Motel.Models;

[Table("service_orders")]
public class ServiceOrder
{
    [Column("id")] public int Id { get; set; }
    [Column("service_id")] public int ServiceId { get; set; }
    [Column("room_id")] public int RoomNumber { get; set; }
    [Column("datetime")] public DateTime Datetime { get; set; }
    
    public virtual Service Service { get; set; } = null!;
    public virtual Room Room { get; set; } = null!;
}

public record ServiceOrderCreateModel(
    int? ServiceId,
    int? RoomNumber,
    DateTime? Datetime
);

public class ServiceOrderCreateModelValidator : AbstractValidator<ServiceOrderCreateModel>
{
    public ServiceOrderCreateModelValidator()
    {
        RuleFor(serviceOrder => serviceOrder.ServiceId).NotNull();
        RuleFor(serviceOrder => serviceOrder.RoomNumber).NotNull();
        RuleFor(serviceOrder => serviceOrder.Datetime).NotNull();
    }
}

public class ServiceOrderPatchModel : PatchDtoBase
{
    private int? serviceId;
    private int? roomNumber;
    private DateTime? datetime;
    
    public int? ServiceId { get => serviceId; set => SetField(ref serviceId, value); }
    public int? RoomNumber { get => roomNumber; set => SetField(ref roomNumber, value); }
    public DateTime? Datetime { get => datetime; set => SetField(ref datetime, value); }
}

public class ServiceOrderPatchModelValidator : AbstractValidator<ServiceOrderPatchModel>
{
    public ServiceOrderPatchModelValidator()
    {
        RuleFor(serviceOrder => serviceOrder.ServiceId).NotNull().WhenPropertyChanged();
        RuleFor(serviceOrder => serviceOrder.RoomNumber).NotNull().WhenPropertyChanged();
        RuleFor(serviceOrder => serviceOrder.Datetime).NotNull().WhenPropertyChanged();
    }
}