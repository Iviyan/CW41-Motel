using System.Text.RegularExpressions;
using NpgsqlTypes;

namespace Motel.Models;

[Table("room_cleaning")]
public class RoomCleaning
{
    [Column("room_id")] public int RoomNumber { get; set; }
    [Column("datetime")] public DateTime Datetime { get; set; }
    [Column("employee_id")] public int EmployeeId { get; set; }
    
    public virtual Room Room { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    
}