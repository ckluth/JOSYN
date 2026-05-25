using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Vertragsdefinition für den JIP-Request-Dispatcher.
/// Routet eingehende JIP-Anfragen anhand von <see cref="Request.What"/> an registrierte Handler
/// und eliminiert manuelle <c>switch</c>-Ausdrücke auf der Serverseite.
/// </summary>
public interface IJipDispatcher
{
    /// <summary>
    /// Die Menge der bisher registrierten Schlüssel.
    /// Gedacht für Test-Assertions, die Protokollvollständigkeit prüfen.
    /// </summary>
    IReadOnlySet<string> RegisteredKeys { get; }

    /// <summary>
    /// Registriert einen asynchronen Handler ohne Eingabedaten mit String-Rückgabe.
    /// Geeignet für Methoden der Form <c>Task&lt;Result&lt;string&gt;&gt; GetXxx()</c>.
    /// </summary>
    IJipDispatcher Register(string key, Func<Task<Result<string>>> handler);

    /// <summary>
    /// Registriert einen asynchronen Handler mit erforderlichem String-Argument ohne Rückgabewert.
    /// Gibt einen Fehler zurück, wenn die Anfrage keine Daten enthält.
    /// Geeignet für Methoden der Form <c>Task&lt;Result&gt; PutXxx(string value)</c>.
    /// </summary>
    IJipDispatcher Register(string key, Func<string, Task<Result>> handler);

    /// <summary>
    /// Registriert einen asynchronen Handler mit optionalem Eingabe- und Ausgabe-String.
    /// Geeignet für die allgemeinste async-Handler-Form.
    /// </summary>
    IJipDispatcher Register(string key, Func<string?, Task<Result<string?>>> handler);

    /// <summary>
    /// Registriert einen synchronen Handler mit optionalem Eingabe- und Ausgabe-String.
    /// Geeignet für Inline-Funktionen der Form <c>(data) => Result&lt;string?&gt;.Success(...)</c>.
    /// </summary>
    IJipDispatcher Register(string key, Func<string?, Result<string?>> handler);

    /// <summary>
    /// Registriert ein konstantes Ergebnis — nützlich für feste Antworten wie PING oder
    /// hart verdrahtete Konfigurationswerte.
    /// </summary>
    IJipDispatcher Register(string key, Result<string?> constantResult);

    /// <summary>
    /// Registriert alle Methoden von <typeparamref name="TProtocol"/> per Konvention:
    /// Der Methodenname wird zum <c>What</c>-Schlüssel, die Signatur bestimmt die Handler-Form.
    /// </summary>
    /// <remarks>
    /// Unterstützte Signaturen:
    /// <list type="bullet">
    /// <item><c>Task&lt;Result&lt;string&gt;&gt; Method()</c></item>
    /// <item><c>Task&lt;Result&gt; Method(string data)</c></item>
    /// </list>
    /// Wirft <see cref="InvalidOperationException"/> bei der Registrierung nicht unterstützter Signaturen.
    /// </remarks>
    /// <typeparam name="TProtocol">
    /// Der Protokoll-Interface-Typ. Immer explizit das Interface angeben, nicht die konkrete Klasse,
    /// um die Registrierung von Nicht-Protokoll-Mitgliedern zu vermeiden.
    /// </typeparam>
    IJipDispatcher RegisterAll<TProtocol>(TProtocol impl) where TProtocol : class;

    /// <summary>
    /// Dispatcht einen rohen JIP-Anfrage-String. Unbekannte <c>What</c>-Werte werden mit
    /// einer Fehler-Response beantwortet.
    /// Diese Methode als <see cref="ServerStartArguments.HandleStringRequest"/> übergeben.
    /// </summary>
    Task<string> Dispatch(string requestStr);
}
