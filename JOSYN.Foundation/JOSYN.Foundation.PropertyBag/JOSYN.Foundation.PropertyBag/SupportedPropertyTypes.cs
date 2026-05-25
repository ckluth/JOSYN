#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Legt die Menge der Eigenschaftstypen fest, die <see cref="PropertyBag"/> serialisieren und deserialisieren kann.
/// </summary>
/// <remarks>
/// Nullable-Wrapper (<c>T?</c>) jedes unterstützten Typs werden ebenfalls akzeptiert.
/// Alle <see langword="enum"/>-Typen werden unabhängig von ihrem zugrunde liegenden Typ unterstützt.
/// </remarks>
internal static class SupportedPropertyTypes
{
    /// <summary>
    /// Ermittelt, ob <paramref name="type"/> ein unterstützter Eigenschaftstyp ist, einschließlich
    /// Nullable-Wrapper und aller <see langword="enum"/>-Typen.
    /// </summary>
    /// <param name="type">Der zu prüfende Eigenschaftstyp.</param>
    /// <returns>
    /// <see langword="true"/>, wenn der Typ von <see cref="PropertyBag"/> verarbeitet werden kann;
    /// andernfalls <see langword="false"/>.
    /// </returns>
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
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(DateOnly),
        typeof(TimeOnly),
        typeof(Guid),
        
        // more when needed...
    ];
}
