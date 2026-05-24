using System.Globalization;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Defines the canonical culture for all JOSYN processes.
/// </summary>
/// <remarks>
/// JOSYN is intentionally culture-fixed: every serialized value (PropertyBag INI/JSON output,
/// dates, decimals, numbers) is written with this culture. A reader using a different culture
/// would silently misparse stored data — so culture must never vary between writer and reader,
/// or across the lifetime of a data store.
/// <para/>
/// This is a compile-time constant, not a runtime configuration value. To change it, update
/// <see cref="Default"/> here and rebuild. Do <b>not</b> make it configurable at runtime:
/// a culture mismatch between a writer and a reader causes silent data-corruption.
/// <para/>
/// Every JOSYN host process is responsible for applying this at startup:
/// <code>
/// CultureInfo.DefaultThreadCurrentCulture   = JosynCulture.Default;
/// CultureInfo.DefaultThreadCurrentUICulture = JosynCulture.Default;
/// </code>
/// Libraries within JOSYN must <b>not</b> set <see cref="CultureInfo.DefaultThreadCurrentCulture"/>
/// themselves — that is the host's responsibility.
/// </remarks>
public static class JosynCulture
{
    /// <summary>
    /// The canonical culture used throughout all JOSYN processes.
    /// Currently <c>de-DE</c>. See class remarks before changing this value.
    /// </summary>
    public static readonly CultureInfo Default = CultureInfo.GetCultureInfo("de-DE");
}
