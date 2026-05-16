#pragma warning disable IDE0130
namespace JOSYN.Core.PropertyBag;
#pragma warning restore IDE0130

internal static class SupportedPropertyTypes
{
    internal static bool IsMatch(Type type)
    {
        var targetType = Nullable.GetUnderlyingType(type) ?? type;
        return targetType.IsEnum || SupportedPropertyTypes.Types.Contains(targetType);
    }

    private static readonly HashSet<Type> Types =
    [
        typeof(string),
        typeof(bool),
        typeof(char),
        typeof(byte),
        typeof(sbyte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(DateTime),
        typeof(TimeSpan),		
        typeof(DateOnly),
        typeof(TimeOnly),
        typeof(Guid),
        
        // more when needed...
    ];
}