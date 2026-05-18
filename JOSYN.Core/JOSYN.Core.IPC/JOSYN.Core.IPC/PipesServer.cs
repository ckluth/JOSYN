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
    public static async Task<Result> RunAsync(string clientExePath, Func<string, Task<string>> processRequest, TimeSpan connectTimeout, string? sessionKey = null, Func<bool>? shouldCancel = null)
    {
        var handler = new RawRequestHandler{ ProcessStrings = processRequest };
        return await RunAsync(clientExePath, handler.ProcessRawRequest, connectTimeout, sessionKey, shouldCancel);
    }

    /// <inheritdoc/>   
    public static async Task<Result> RunAsync(string clientExePath, Func<byte[], Task<byte[]>> processRequest, TimeSpan connectTimeout, string? sessionKey = null, Func<bool>? shouldCancel = null)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionKey)) sessionKey = Guid.NewGuid().ToString();

            var startClient = StartClientExe(clientExePath, sessionKey);
            if (!startClient.Succeeded)
                return Result.Propagate(startClient);

            var (disposeHandler, cancellationToken) = CreatePollingCancellationToken(shouldCancel);
            var res = await RunAsync(processRequest, connectTimeout, sessionKey, cancellationToken);
            disposeHandler?.Invoke();
            return res;

        }
        catch (Exception ex) { return ex; }
    }

    /// <inheritdoc/>   
    public static async Task<Result> RunAsync(Func<string, Task<string>> processRequest, TimeSpan connectTimeout, string sessionKey, Func<bool>? shouldCancel = null)
    {
        var handler = new RawRequestHandler { ProcessStrings = processRequest };
        var (disposeHandler, cancellationToken) = CreatePollingCancellationToken(shouldCancel);
        var res = await RunAsync(handler.ProcessRawRequest, connectTimeout, sessionKey, cancellationToken);
        disposeHandler?.Invoke();
        return res;
    }
    
    /// <inheritdoc/>   
    public static async Task<Result> RunAsync(Func<byte[], Task<byte[]>> processRequest, TimeSpan connectTimeout, string sessionKey, Func<bool>? shouldCancel = null)
    {
        var (disposeHandler, cancellationToken) = CreatePollingCancellationToken(shouldCancel);
        var res = await RunAsync(processRequest, connectTimeout, sessionKey, cancellationToken);
        disposeHandler?.Invoke();
        return res;
    }
    
    /// <inheritdoc/>   
    public static async Task<Result> RunAsync(Func<byte[], Task<byte[]>> processRequest, TimeSpan connectTimeout, string sessionKey, CancellationToken cancellationToken = default)
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
                    conn.RequestPipe, conn.ResponsePipe, processRequest, cancellationToken);
            }
        }
        catch (Exception ex) { return ex; }
    }

    #region private

    internal class RawRequestHandler
    {
        internal required Func<string, Task<string>> ProcessStrings { get; set; }

        internal async Task<byte[]> ProcessRawRequest(byte[] requestBytes)
        {
            var requestStr = Encoding.UTF8.GetString(requestBytes);
            var responseStr = await ProcessStrings(requestStr);
            return Encoding.UTF8.GetBytes(responseStr);
        }
    }

    private static (Action? disposeHandler, CancellationToken cancellationToken) CreatePollingCancellationToken(Func<bool>? shouldCancel = null, int pollIntervalMs = 100)
    {
        if (shouldCancel == null) 
            return (null, CancellationToken.None);
        
        var cts = new CancellationTokenSource();
        Action cancel = cts.Cancel;

        // ReSharper disable once MethodSupportsCancellation
        _ = Task.Run(() =>
        {
            while (!cts.IsCancellationRequested)
            {
                if (shouldCancel())
                {
                    cts.Cancel();
                    break;
                }
                Thread.Sleep(pollIntervalMs);
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

    private static async Task<Result> RequestLoopAsync(
        NamedPipeServerStream reqPipe,
        NamedPipeServerStream resPipe,
        Func<byte[], Task<byte[]>> processRequest,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // BinaryReader/BinaryWriter used consistently on both sides.
            // Note: BinaryWriter.Write(byte[]) emits a length prefix before the bytes, which
            // would create a double-prefix bug. The correct overload is Write(byte[], 0, n),
            // which writes raw bytes only — matching BinaryReader.ReadBytes(n) on the client.
            using var reader = new BinaryReader(reqPipe,  Encoding.UTF8, leaveOpen: true);
            using var writer = new BinaryWriter(resPipe, Encoding.UTF8, leaveOpen: true);

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
                    return Result.Error("Request-Loop durch Aufrufer abgebrochen.");
                }
                catch (EndOfStreamException)
                {
                    break;
                }

                var response = await processRequest(requestBytes);
                writer.Write(response.Length);
                writer.Write(response, 0, response.Length); // raw bytes only — no extra length prefix
            }
            return Result.Success;
        }
        catch (Exception ex) { return Result.Fail(ex); }
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