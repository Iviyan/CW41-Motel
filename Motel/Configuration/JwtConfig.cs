namespace Motel.Configuration;

public class JwtConfig
{
    public const string SectionName = "Jwt";
    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = null!;

    public SymmetricSecurityKey SecretKey => new(Encoding.UTF8.GetBytes(Secret));
}