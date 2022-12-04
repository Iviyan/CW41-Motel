using System.Text.RegularExpressions;
using NpgsqlTypes;

namespace Motel.Models;

[Table("lease_agreements")]
public class LeaseAgreement
{
    [Column("id")] public int Id { get; set; }
    [Column("client_name")] public required string ClientName { get; set; }
    [Column("start_at")] public DateTime StartAt { get; set; }
    [Column("end_at")] public DateTime EndAt { get; set; }
    
    [JsonIgnore] public virtual ICollection<LeaseRoom> LeaseRooms { get; set; } = null!;
}

public record LeaseAgreementCreateModel(
    string? ClientName,
    DateTime? StartAt,
    DateTime? EndAt,
    int[]? Rooms
);

public class LeaseAgreementCreateModelValidator : AbstractValidator<LeaseAgreementCreateModel>
{
    private static readonly Regex ClientNameRegex =
        new(@"^[а-яa-z- ]+$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    public LeaseAgreementCreateModelValidator()
    {
        RuleFor(leaseAgreement => leaseAgreement.ClientName).NotNull()
            .Matches(ClientNameRegex).WithMessage("{PropertyName} must contain only alphabetic characters space and dash");
        RuleFor(leaseAgreement => leaseAgreement.StartAt).NotNull();
        RuleFor(leaseAgreement => leaseAgreement.EndAt).NotNull().GreaterThan(l => l.StartAt);
        RuleFor(leaseAgreement => leaseAgreement.Rooms).NotNull()
            .Must(l => l?.Length > 0).WithMessage("The 'rooms' list must contain at least one value");
    }
}

public class LeaseAgreementPatchModel : PatchDtoBase
{
    private string? clientName;
    private DateTime? startAt;
    private DateTime? endAt;

    public string? ClientName { get => clientName; set => SetField(ref clientName, value); }
    public DateTime? StartAt { get => startAt; set => SetField(ref startAt, value); }
    public DateTime? EndAt { get => endAt; set => SetField(ref endAt, value); }
}

public class LeaseAgreementPatchModelValidator : AbstractValidator<LeaseAgreementPatchModel>
{
    private static readonly Regex ClientNameRegex =
        new(@"^[а-яa-z- ]+$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    public LeaseAgreementPatchModelValidator()
    {
        RuleFor(leaseAgreement => leaseAgreement.ClientName).NotNull()
            .Matches(ClientNameRegex).WithMessage("{PropertyName} must contain only alphabetic characters space and dash")
            .WhenPropertyChanged();
        RuleFor(leaseAgreement => leaseAgreement.StartAt).NotNull().WhenPropertyChanged();
        RuleFor(leaseAgreement => leaseAgreement.EndAt).NotNull().GreaterThan(l => l.StartAt).WhenPropertyChanged();
    }
}