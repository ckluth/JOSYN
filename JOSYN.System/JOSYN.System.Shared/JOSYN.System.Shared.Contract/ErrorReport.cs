namespace JOSYN.System.Shared.Contract;

/// <summary>
/// Strukturierter Fehlerbericht, den der JobHost (Frontend) an den JAPServer (Backend)
/// übermittelt, wenn ein Job-Fehler aufgetreten ist, der Pipe-Transport aber noch
/// funktionsfähig war.
/// Wird via PropertyBag serialisiert und als roher String über
/// <see cref="IJosynApplicationProtocol.PutError"/> übertragen.
/// </summary>
public record ErrorReport(
    string  Causer,
    string  Message,
    string? CallStack,
    string? ExceptionDetails,
    DateTimeOffset OccurredAt);
