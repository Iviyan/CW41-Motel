namespace Motel.Utils;
public record ValueDescription<T>(T Value, string Description) where T: struct, Enum;

public static class EnumHelper
{
    public static string Description(this Enum value)
    {
        var attributes = value.GetType().GetField(value.ToString())!.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attributes.Any())
            return (attributes.First() as DescriptionAttribute)!.Description;

        return value.ToString("G");
    }

    public static ValueDescription<T>[] Get<T>() where T : struct, Enum =>
        Enum.GetValues<T>()
            .Select(e => new ValueDescription<T>(e, e.Description()))
            .OrderBy(e => e.Value)
            .ToArray();
}