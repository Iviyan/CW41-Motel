using System.Text.RegularExpressions;
using NpgsqlTypes;

namespace Motel.Models;

[Table("lease_rooms")]
public class LeaseRoom
{
    [Column("lease_agreement_id")] public int LeaseAgreementId { get; set; }
    [Column("room_id")] public int RoomNumber { get; set; }
    
    public virtual LeaseAgreement LeaseAgreement { get; set; } = null!;
    public virtual Room Room { get; set; } = null!;
}