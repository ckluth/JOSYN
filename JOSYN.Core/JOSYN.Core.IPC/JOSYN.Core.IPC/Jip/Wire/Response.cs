using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

#pragma warning disable IDE0130
namespace JOSYN.Core.IPC.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Ausgehende JIP-Antwort (Wire Format). Serialisierung über <see cref="JipProtocol"/>.
/// </summary>
public record Response : IResponse
{
    /// <inheritdoc/>
    public ResponseStatus Status { get; init; }

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(Error))]
    public bool HasError => Status is ResponseStatus.TechnicalFailure or ResponseStatus.LogicalFailure;

    /// <inheritdoc/>
    public string? Error { get; init; }

    /// <inheritdoc/>
    public string? Data { get; init; }

    /// <inheritdoc/>
    public Dictionary<string, string>? Dict { get; init; }

    /// <inheritdoc/>
    public override string ToString() => JsonSerializer.Serialize(this);
}