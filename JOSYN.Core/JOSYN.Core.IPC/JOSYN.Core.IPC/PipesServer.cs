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
    public static async Task<Result> RunAsync(string clientExePath, Func<string, Task<string>> processRequest, TimeSpan connectTimeout,
        Func<string, Exception, Task> onError, string? sessionKey = null, Func<Task<bool>>? shouldCancel = null)
    {
        var handler = new RawRequestHandler{ ProcessStrings = processRequest };
        return await RunAsync(clientExePath, handler.ProcessRawRequest, connectTimeout, onError, sessionKey, shouldCancel);
    }

    /// <inheritdoc/>   
    public static async Task<Result> RunAsync(string clientExePath, Func<byte[], Task<byte[]>> processRequest, TimeSpan connectTimeout,
        Func<string, Exception, Task> onError, string? sessionKey = null, Func<Task<bool>>? shouldCancel = null)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionKey)) sessionKey = Guid.NewGuid().ToString();

            var startClient = StartClientExe(clientExePath, sessionKey);
            if (!startClient.Succeeded)
                return Result.Propagate(startClient);

            var (disposeHandler, cancellationToken) = CreatePollingCancellationToken(shouldCancel);
            var res = await RunAsync(processRequest, connectTimeout, onError, sessionKey, cancellationToken);
            disposeHandler?.Invoke();
            return res;

        }
        catch (Exception ex) { return ex; }
    }

    /// <inheritdoc/>   
    public static async Task<Result> RunAsync(Func<string, Task<string>> processRequest, TimeSpan connectTimeout,
        Func<string, Exception, Task> onError, string sessionKey, Func<Task<bool>>? shouldCancel = null)
    {
        var handler = new RawRequestHandler { ProcessStrings = processRequest };
        var (disposeHandler, cancellationToken) = CreatePollingCancellationToken(shouldCancel);
        var res = await RunAsync(handler.ProcessRawRequest, connectTimeout, onError, sessionKey, cancellationToken);
        disposeHandler?.Invoke();
        return res;
    }
    
    /// <inheritdoc/>   
    public static async Task<Result> RunAsync(Func<byte[], Task<byte[]>> processRequest, TimeSpan connectTimeout,
        Func<string, Exception, Task> onError, string sessionKey, Func<Task<bool>>? shouldCancel = null)
    {
        var (disposeHandler, cancellationToken) = CreatePollingCancellationToken(shouldCancel);
        var res = await RunAsync(processRequest, connectTimeout, onError, sessionKey, cancellationToken);
        disposeHandler?.Invoke();
        return res;
    }
 
    /// <inheritdoc/>   
    public static async Task<Result> RunAsync(Func<byte[], Task<byte[]>> processRequest, TimeSpan connectTimeout,
        Func<string, Exception, Task> onError, string sessionKey, CancellationToken cancellationToken = default)
    {
	    // Die eigentliche Implementierug.
        // Alle anderen public Überladungen delegieren hierhin...
        try
        {
            var getConnection = CreatePipes(sessionKey);

            if (!getConnection.Succeeded)
                return Result.Propagate(getConnection.ToResult());

            var conn = getConnection.Value;

            await using (conn.RequestPipe)
            await using (conn.ResponsePipe)
            {
                using var connectCts = new CancellationTokenSource(connectTimeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    connectCts.Token, cancellationToken);

                try
                {
                    await Task.WhenAll(
                        conn.RequestPipe.WaitForConnectionAsync(linkedCts.Token),
                        conn.ResponsePipe.WaitForConnectionAsync(linkedCts.Token));
                }
                catch (OperationCanceledException)
                {
                    return Result.Error(cancellationToken.IsCancellationRequested
                        ? "Verbindung durch Aufrufer abgebrochen."
                        : "Timeout: kein Client verbunden.");
                }

                return await RequestLoopAsync(
                    conn.RequestPipe, conn.ResponsePipe, processRequest, onError, cancellationToken);
            }
        }
        catch (Exception ex) { return ex; }
    }

    #region private

    private class RawRequestHandler
    {
        internal required Func<string, Task<string>> ProcessStrings { get; init; }

        internal async Task<byte[]> ProcessRawRequest(byte[] requestBytes)
        {
            var requestStr = Encoding.UTF8.GetString(requestBytes);
            var responseStr = await ProcessStrings(requestStr);
            return Encoding.UTF8.GetBytes(responseStr);
        }
    }

    private static (Action? disposeHandler, CancellationToken cancellationToken) CreatePollingCancellationToken(Func<Task<bool>>? shouldCancel = null, int pollIntervalMs = 500)
    {
        // TODO: fixes Poll-Interval 500ms dokumentieren
        
        if (shouldCancel == null) 
            return (null, CancellationToken.None);
        
        var cts = new CancellationTokenSource();
        Action cancel = cts.Cancel;

        // ReSharper disable once MethodSupportsCancellation
        _ = Task.Run(async() =>
        {
            while (!cts.IsCancellationRequested)
            {
                // Eine Exception in shouldCancel() würde hier diesen Task beenden, ohne dass cts.Cancel() aufgerufen wird.
                // Die Exception "verschwindet im Nirwana".
                // "Könnte man" aufangen - und propagieren, oder als CancelRequest behandeln - aber NOPE!
                // Explizite Design-Entscheidung hier:
                // Der IsCancellationRequested-Callback in der Anwendung soll lightweigth und schlank implementiert
                // sein und keinen kritischen Code beinhalten! Bei Verstpß gegen dieses dokumentierte Konzept: Ein Fall von "Pech gehabt"!

                var isCanncellationRequested = await shouldCancel();
                
                if (isCanncellationRequested)
                {
                    await cts.CancelAsync();
                    break;
                }
                // ReSharper disable once MethodSupportsCancellation
                await Task.Delay(pollIntervalMs);
            }
        });
        
        return (() => { cancel?.Invoke(); cts.Dispose(); }, cts.Token);
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

    //----------------------------------------------------------------------------------------------------
    // DISCLAIMER: Single-in-flight — kein Multiplexing im zentralen Request-Loop!
    //
    // Wie in der Spec des JOSYN-IPC-Protocols als design-basierte Limitierung beschrieben:
    // Der Request-Loop verarbeitet Anfragen strikt sequenziell.
    // Parallele Requests könne zu undefiniertem Verhalten führen.
    //
    // CALM DOWN: "Will never happen" in der internen JOSYN-Implementierung. ;)		
    //
    // TODO: "undefiniertes Verhalten" ist schon blöd; eine BUSY/Rejected-Antwort muss doch drin sein...
    //----------------------------------------------------------------------------------------------------

    private static async Task<Result> RequestLoopAsync(
        NamedPipeServerStream reqPipe,
        NamedPipeServerStream resPipe,
        Func<byte[], Task<byte[]>> processRequest,
        Func<string, Exception, Task> onError,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // BinaryReader/BinaryWriter used consistently on both sides.
            // Note: BinaryWriter.Write(byte[]) emits a length prefix before the bytes, which
            // would create a double-prefix bug. The correct overload is Write(byte[], 0, n),
            // which writes raw bytes only — matching BinaryReader.ReadBytes(n) on the client.
            using var reader = new BinaryReader(reqPipe,  Encoding.UTF8, leaveOpen: true);
            await using var writer = new BinaryWriter(resPipe, Encoding.UTF8, leaveOpen: true);

            while (reqPipe.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                byte[] requestBytes;
                try
                {
                    var messageLength = reader.ReadInt32();
                    requestBytes = reader.ReadBytes(messageLength);
                }				
                catch (OperationCanceledException)
                {
					// can't happen in the current design - but, paranoia...
                    return Result.Error("Request-Loop durch Aufrufer abgebrochen.");
                }
                catch (EndOfStreamException)
                {
                    break;
                }

                byte[] response;
                try
                {
                    response = await processRequest(requestBytes);
                }
                catch (Exception ex)
                {
                    await onError(Encoding.UTF8.GetString(requestBytes), ex);
                    var responseStr = $"{PipesProtocol.MagicErrorResponsePrefix}{ex}";
                    response = Encoding.UTF8.GetBytes(responseStr);
                }

                writer.Write(response.Length);
                writer.Write(response, 0, response.Length); // raw bytes only — no extra length prefix
				writer.Flush();
            }
            return Result.Success;
        }
        catch (Exception ex) { return ex; }
    }


    private static Result<ServerPipes> CreatePipes(string sessionKey)
    {
        var (requestPipeName, responsePipeName) = PipesProtocol.DerivePipeNamesFromSessionKey(sessionKey);
        return CreatePipes(requestPipeName, responsePipeName);
    }

    private static Result<ServerPipes> CreatePipes(string requestPipeName, string responsePipeName)
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

            return new ServerPipes { RequestPipe = reqPipe, ResponsePipe = resPipe };

        }
        catch (Exception ex) { return ex; }
    }

    #endregion

}