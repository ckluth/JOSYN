using System.IO.Pipes;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Vertragsdefinition für das Server-seitige Named-Pipe-Handle.
/// Kapselt die zwei Pipes (Request und Response) einer aktiven JIP-Server-Verbindung.
/// </summary>
public interface IServerPipes
{
    /// <summary>Named Pipe, über die der Server Anfragen vom Client empfängt.</summary>
    NamedPipeServerStream RequestPipe { get; init; }

    /// <summary>Named Pipe, über die der Server Antworten an den Client sendet.</summary>
    NamedPipeServerStream ResponsePipe { get; init; }
}
