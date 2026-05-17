using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using JOSYN.Core.ResultPattern;

namespace JOSYN.Core.IPC;

/// <summary>
/// TODO 
/// </summary>
public class PipesServer : IPipesServer
{
    /// <inheritdoc/>   
    public static async Task<Result> RunAsync(string clientExePath, Func<string, string> processRequest, TimeSpan connectTimeout, string? sessionKey = null)
    {
        var handler = new RawRequestHandler{ ProcessStrings = processRequest };
        return await RunAsync(clientExePath, handler.ProcessRawRequest, connectTimeout, sessionKey);
    }

    /// <inheritdoc/>   
    public static async Task<Result> RunAsync(Func<string, string> processRequest, TimeSpan connectTimeout, string sessionKey)
    {
        var handler = new RawRequestHandler { ProcessStrings = processRequest };
        return await RunAsync(handler.ProcessRawRequest, connectTimeout, sessionKey);
    }

    /// <inheritdoc/>   
    public static async Task<Result> RunAsync(string clientExePath, Func<byte[], byte[]> processRequest, TimeSpan connectTimeout, string? sessionKey = null)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionKey)) sessionKey = Guid.NewGuid().ToString();

            var startClient = StartClientExe(clientExePath, sessionKey);
            if (!startClient.Succeeded)
                return Result.Propagate(startClient);

            return await RunAsync(processRequest, connectTimeout, sessionKey);
        }
        catch (Exception ex) { return ex; }
    }

    /// <inheritdoc/>   
    public static async Task<Result> RunAsync(Func<byte[], byte[]> processRequest, TimeSpan connectTimeout, string sessionKey)
    {
        try
        {
            var getConnection = await CreatePipesAsync(sessionKey);

            if (!getConnection.Succeeded)
                return Result.Propagate(getConnection.ToResult());

            var conn = getConnection.Value;

            await using (conn.RequestPipe)
            await using (conn.ResponsePipe)
            {
                var connectCts = new CancellationTokenSource(connectTimeout);
                try
                {
                    await Task.WhenAll(
                        conn.RequestPipe.WaitForConnectionAsync(connectCts.Token),
                        conn.ResponsePipe.WaitForConnectionAsync(connectCts.Token));
                }
                catch (OperationCanceledException)
                {
                    return Result.Error("Timeout: kein Client verbunden.");
                }

                // Keine Unterstützung von CancellationToken in der einmal establishten Connection.
                // Schleife läuft, bis der Client die Verbindung - explizt oder durch Terminieren - schließt; oder ein Fehler auftritt.
                var result = await RequestLoopAsync(conn.RequestPipe, conn.ResponsePipe, processRequest);
                
                return result;
            }
        }

        catch (Exception ex) { return ex; }
    }

    #region private

    internal class RawRequestHandler
    {
        internal required Func<string, string> ProcessStrings { get; set; }

        internal byte[] ProcessRawRequest(byte[] requestBytes)
        {
            var requestStr = Encoding.UTF8.GetString(requestBytes);
            var responseStr = ProcessStrings(requestStr);
            return Encoding.UTF8.GetBytes(responseStr);
        }
    }


    private static Result StartClientExe(string remoteExePath, string sessionKey)
    {
        if (!File.Exists(remoteExePath))
            return Result.Error($"Client-Exe not found: {remoteExePath}");

        try
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = remoteExePath,
                Arguments = PipesProtocol.CreateClientStartCLIArguments(sessionKey),
                UseShellExecute = true,
                CreateNoWindow = false
            });

            return (p == null)
                ? Result.Error($"Failed to start client process: {remoteExePath}") 
                : Result.Success;
        }
        catch (Exception ex) { return ex; }
    }

    private static async Task<Result> RequestLoopAsync(NamedPipeServerStream reqPipe, NamedPipeServerStream resPipe, Func<byte[], byte[]> processRequest)
    {
        try
        {
            await using var writer = new BinaryWriter(resPipe, System.Text.Encoding.UTF8, leaveOpen: true);

            while (reqPipe.IsConnected)
            {
                byte[] requestBytes;
                try
                {
                    var lengthPrefix = new byte[4];
                    await reqPipe.ReadExactlyAsync(lengthPrefix, 0, 4);
                    var messageLength = BitConverter.ToInt32(lengthPrefix, 0);
                    requestBytes = new byte[messageLength];
                    await reqPipe.ReadExactlyAsync(requestBytes, 0, messageLength);
                }
                catch (EndOfStreamException)
                {
                    // Passiert auch bei regulärem Schließen der Verbindung durch den Client.
                    // Daher kein Fehler, sondern einfach Verbindungsende.
                    break;
                }

                var response = processRequest(requestBytes);
                
                writer.Write(response.Length);
                writer.Write(response);
            }
            return Result.Success;
        }

        catch (Exception ex) { return Result.Fail(ex); }
    }

    private static async Task<Result<ServerPipes>> CreatePipesAsync(string sessionKey)
    {
        var (requestPipeName, responsePipeName) = PipesProtocol.DerivePipeNamesFromSessionKey(sessionKey);
        return await CreatePipesAsync(requestPipeName, responsePipeName);
    }

    private static async Task<Result<ServerPipes>> CreatePipesAsync(string requestPipeName, string responsePipeName)
    {
        try
        {
            var reqPipe = new NamedPipeServerStream(
                pipeName: requestPipeName,
                direction: PipeDirection.In,
                maxNumberOfServerInstances: 1,
                transmissionMode: PipeTransmissionMode.Byte,
                options: PipeOptions.Asynchronous);

            var resPipe = new NamedPipeServerStream(
                pipeName: responsePipeName,
                direction: PipeDirection.Out,
                maxNumberOfServerInstances: 1,
                transmissionMode: PipeTransmissionMode.Byte,
                options: PipeOptions.Asynchronous);

            return await Task.FromResult(new ServerPipes { RequestPipe = reqPipe, ResponsePipe = resPipe });

        }
        catch (Exception ex) { return ex; }
    }

    #endregion

}