using System.IO.Pipes;

#pragma warning disable IDE0130
namespace JOSYN.Core.IPC;
#pragma warning restore IDE0130

/// <summary>
/// TODO
/// </summary>
public record ServerPipes
{
    /// <summary>
    /// TODO
    /// </summary>
    public required NamedPipeServerStream RequestPipe { get; init; }

    /// <summary>
    /// TODO
    /// </summary>
    public required NamedPipeServerStream ResponsePipe { get; init; }
}