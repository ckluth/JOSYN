using System.Text.Json;

#pragma warning disable IDE0130
namespace JOSYN.Core.IPC.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Eingehende JIP-Anfrage (Wire Format). Serialisierung über <see cref="JipProtocol"/>.
/// </summary>
public record Request : IRequest
{
    /// <inheritdoc/>
    public required string What { get; init; }

    /// <inheritdoc/>
    public string? Data { get; init; }

    /// <inheritdoc/>
    public override string ToString() => JsonSerializer.Serialize(this);
}