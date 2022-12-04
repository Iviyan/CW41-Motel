namespace Motel;

public class RequestData
{
    public int? UserId { get; set; }
    public string? UserLogin { get; set; }
    public Guid DeviceUid { get; set; }
    public Posts? Post { get; set; }
}