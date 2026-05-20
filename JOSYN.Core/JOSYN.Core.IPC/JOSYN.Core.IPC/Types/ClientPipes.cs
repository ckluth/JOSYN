using System.IO.Pipes;

#pragma warning disable IDE0130
namespace JOSYN.Core.IPC;
#pragma warning restore IDE0130

/// <summary>
/// TODO
/// </summary>
public sealed class ClientPipes
{
    /// <summary>
    /// TODO
    /// </summary>
    public required NamedPipeClientStream RequestPipe { get; init; }

    /// <summary>
    /// TODO
    /// </summary>
    public required NamedPipeClientStream ResponsePipe { get; init; }
    
    private int isBusy;

    /// <summary>Atomically claims the busy-slot. Returns true if the caller acquired it, false if already busy.</summary>
    internal bool TrySetBusy() => Interlocked.CompareExchange(ref isBusy, 1, 0) == 0;

    internal void ClearBusy() => Interlocked.Exchange(ref isBusy, 0);
}