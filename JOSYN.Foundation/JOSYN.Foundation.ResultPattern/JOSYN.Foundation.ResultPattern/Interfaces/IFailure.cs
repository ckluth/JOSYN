#pragma warning disable IDE0130
namespace JOSYN.Foundation.ResultPattern;
#pragma warning restore IDE0130

/// <summary>
/// Vertrag für den <see cref="Error"/>-Werttyp.
/// </summary>
public interface IError<out TSelf> where TSelf : IError<TSelf>
{
    /// <summary>
    /// Die Fehlermeldung.
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    /// Die auslösende Ausnahme, falls vorhanden.
    /// </summary>
    Exception? Exception { get; }

    /// <summary>
    /// Ermöglicht <c>Error err = "Meldung";</c>.
    /// </summary>
    static abstract implicit operator TSelf(string error);

    /// <summary>
    /// Ermöglicht <c>Error err = exception;</c>.
    /// </summary>
    static abstract implicit operator TSelf(Exception exception);
}
