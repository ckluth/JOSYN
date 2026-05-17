using System.IO.Pipes;

#pragma warning disable IDE0130
namespace JOSYN.Core.IPC;
#pragma warning restore IDE0130

/// <summary>
/// TODO
/// </summary>
public record ClientPipes
{
    /// <summary>
    /// TODO
    /// </summary>
    public required NamedPipeClientStream RequestPipe { get; init; }

    /// <summary>
    /// TODO
    /// </summary>
    public required NamedPipeClientStream ResponsePipe { get; init; }
}