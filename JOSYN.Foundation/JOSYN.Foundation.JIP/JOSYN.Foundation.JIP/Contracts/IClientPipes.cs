using System.IO.Pipes;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Vertragsdefinition für das Client-seitige Named-Pipe-Handle.
/// Kapselt die zwei Pipes (Request und Response) einer aktiven JIP-Client-Verbindung.
/// </summary>
public interface IClientPipes
{
    /// <summary>Named Pipe, über die der Client Anfragen an den Server sendet.</summary>
    NamedPipeClientStream RequestPipe { get; init; }

    /// <summary>Named Pipe, über die der Client Antworten vom Server empfängt.</summary>
    NamedPipeClientStream ResponsePipe { get; init; }
}
