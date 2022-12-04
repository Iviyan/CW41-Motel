namespace Motel.Configuration;

public class AdminUserConfig
{
    public const string SectionName = "AdminUser";
    
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
}