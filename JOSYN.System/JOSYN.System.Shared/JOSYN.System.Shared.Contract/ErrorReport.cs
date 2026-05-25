namespace JOSYN.System.Shared.Contract;

/// <inheritdoc cref="IErrorReport"/>
public record ErrorReport(
    string  Causer,
    string  Message,
    string? CallStack,
    string? ExceptionDetails,
    DateTimeOffset OccurredAt) : IErrorReport;
