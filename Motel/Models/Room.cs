using System.Text.RegularExpressions;
using NpgsqlTypes;

namespace Motel.Models;

[Table("rooms")]
public class Room
{
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.None)] public int Number { get; set; }
    [Column("is_cleaning_needed")] public bool IsCleaningNeeded { get; set; }
    [Column("is_ready")] public bool IsReady { get; set; }
    [Column("room_type_id")] public int RoomTypeId { get; set; }
    
    public virtual RoomType RoomType { get; set; } = null!;
    
    [JsonIgnore] public virtual ICollection<RoomCleaning> RoomCleanings { get; } = null!;
    [JsonIgnore] public virtual ICollection<ServiceOrder> ServiceOrders { get; } = null!;
    
    [JsonIgnore] public virtual ICollection<LeaseRoom> LeaseRooms { get; } = null!;
}

public record RoomCreateModel(
    int? Number,
    int? RoomTypeId,
    bool? IsCleaningNeeded = false,
    bool? IsReady = true
);

public class RoomCreateModelValidator : AbstractValidator<RoomCreateModel>
{
    public RoomCreateModelValidator()
    {
        RuleFor(room => room.Number).NotNull();
        RuleFor(room => room.IsCleaningNeeded).NotNull();
        RuleFor(room => room.IsReady).NotNull();
        RuleFor(room => room.RoomTypeId).NotNull();
    }
}

public class RoomPatchModel : PatchDtoBase
{
    private int? number;
    private int? roomTypeId;
    private bool? isCleaningNeeded;
    private bool? isReady;
    
    public int? Number { get => number; set => SetField(ref number, value); }
    public int? RoomTypeId { get => roomTypeId; set => SetField(ref roomTypeId, value); }
    public bool? IsCleaningNeeded { get => isCleaningNeeded; set => SetField(ref isCleaningNeeded, value); }
    public bool? IsReady { get => isReady; set => SetField(ref isReady, value); }
}

public class RoomPatchModelValidator : AbstractValidator<RoomPatchModel>
{
    public RoomPatchModelValidator()
    {
        RuleFor(room => room.Number).NotNull().WhenPropertyChanged();
        RuleFor(room => room.RoomTypeId).NotNull().WhenPropertyChanged();
        RuleFor(room => room.IsCleaningNeeded).NotNull().WhenPropertyChanged();
        RuleFor(room => room.IsReady).NotNull().WhenPropertyChanged();
    }
}