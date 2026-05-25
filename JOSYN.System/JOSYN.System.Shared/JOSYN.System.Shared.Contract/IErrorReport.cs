namespace JOSYN.System.Shared.Contract;

/// <summary>
/// Vertragsdefinition für einen strukturierten Fehlerbericht.
/// Wird vom JobHost (Frontend) an den JAPServer (Backend) übermittelt,
/// wenn ein Job-Fehler aufgetreten ist, der Pipe-Transport aber noch funktionsfähig war.
/// </summary>
public interface IErrorReport
{
    /// <summary>Bezeichner des fehlgeschlagenen Jobs oder Prozesses.</summary>
    string Causer { get; init; }

    /// <summary>Fehlermeldung.</summary>
    string Message { get; init; }

    /// <summary>Optionaler serialisierter Call-Stack.</summary>
    string? CallStack { get; init; }

    /// <summary>Optionale serialisierte Exception-Details.</summary>
    string? ExceptionDetails { get; init; }

    /// <summary>Zeitpunkt des Auftretens.</summary>
    DateTimeOffset OccurredAt { get; init; }
}
