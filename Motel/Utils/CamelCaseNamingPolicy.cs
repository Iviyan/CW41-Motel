namespace Motel.Utils;

public class CamelCaseNamingPolicy : JsonNamingPolicy
{
    public static CamelCaseNamingPolicy Instance { get; } = new();

    public override string ConvertName(string name) => ToCamelCase(name);

    public static string ToCamelCase(string name)
    {
        if (String.IsNullOrWhiteSpace(name)) return name;

        if (name.Length == 1) return name.ToLowerInvariant();

        return char.ToLowerInvariant(name[0]) + name[1..];
    }
    
    public static string FromCamelToPascalCase(string name)
    {
        if (String.IsNullOrWhiteSpace(name)) return name;

        if (name.Length == 1) return name.ToUpperInvariant();

        return char.ToUpperInvariant(name[0]) + name[1..];
    }
}