using System.Text.RegularExpressions;
using NpgsqlTypes;

namespace Motel.Models;

[Table("room_types")]
public class RoomType
{
    [Column("id")] public int Id { get; set; }
    [Column("name")] public required string Name { get; set; }
    [Column("price_per_hour", TypeName = "money")] public double PricePerHour { get; set; }
    [Column("capacity")] public short Capacity { get; set; }

    [JsonIgnore] public virtual ICollection<Room> Rooms { get; } = null!;
}

public record RoomTypeCreateModel(
    string? Name,
    double? PricePerHour,
    short? Capacity
);

public class RoomTypeCreateModelValidator : AbstractValidator<RoomTypeCreateModel>
{
    public RoomTypeCreateModelValidator()
    {
        RuleFor(roomType => roomType.Name).NotNull().MaximumLength(50);
        RuleFor(roomType => roomType.PricePerHour).NotNull().GreaterThan(0);
        RuleFor(roomType => roomType.Capacity).NotNull().GreaterThan((short)0);
    }
}

public class RoomTypePatchModel : PatchDtoBase
{
    private string? name;
    private double? pricePerHour;
    private short? capacity;

    public string? Name { get => name; set => SetField(ref name, value); }
    public double? PricePerHour { get => pricePerHour; set => SetField(ref pricePerHour, value); }
    public short? Capacity { get => capacity; set => SetField(ref capacity, value); }
}

public class RoomTypePatchModelValidator : AbstractValidator<RoomTypePatchModel>
{
    public RoomTypePatchModelValidator()
    {
        RuleFor(roomType => roomType.Name).NotNull().MaximumLength(50).WhenPropertyChanged();
        RuleFor(roomType => roomType.PricePerHour).NotNull().GreaterThan(0).WhenPropertyChanged();
        RuleFor(roomType => roomType.Capacity).NotNull().GreaterThan((short)0).WhenPropertyChanged();
    }
}