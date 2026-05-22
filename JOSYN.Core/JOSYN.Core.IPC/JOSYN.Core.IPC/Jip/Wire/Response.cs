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
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Succeeded { get; init; }

    /// <inheritdoc/>
    public string? Error { get; init; }

    /// <inheritdoc/>
    public string? Data { get; init; }

    /// <inheritdoc/>
    public override string ToString() => JsonSerializer.Serialize(this);
}