using JOSYN.Core.ResultPattern;
using System.IO.Pipes;
using System.Text;

namespace JOSYN.Core.IPC;

/// <summary>
/// TODO
/// </summary>
public class PipesClient : IPipesClient
{
    /// <inheritdoc /> 
    public static async Task<Result<ClientPipes>> ConnectAsync(string sessionKey)
    {
        var (requestPipeName, responsePipeName) = PipesProtocol.DerivePipeNamesFromSessionKey(sessionKey);
        return await ConnectAsync(requestPipeName, responsePipeName);
    }

    /// <inheritdoc /> 
    public static async Task<Result<string>> SendRequestAsync(string request, ClientPipes pipes)
    {
        var result = await SendRequestAsync(Encoding.UTF8.GetBytes(request), pipes);
        if (!result.Succeeded) return Result<string>.Propagate(result.ToResult<string>());
        return Encoding.UTF8.GetString(result.Value);
    }

    /// <inheritdoc /> 
    public static async Task<Result<byte[]>> SendRequestAsync(byte[] requestBytes, ClientPipes pipes)
    {
        try
        {
            var lengthPrefix = BitConverter.GetBytes(requestBytes.Length);
            await pipes.RequestPipe.WriteAsync(lengthPrefix);
            await pipes.RequestPipe.WriteAsync(requestBytes);
            await pipes.RequestPipe.FlushAsync();
            var responseLengthBytes = new byte[4];
            await pipes.ResponsePipe.ReadExactlyAsync(responseLengthBytes, 0, 4);
            var responseLength = BitConverter.ToInt32(responseLengthBytes, 0);
            var responseBytes = new byte[responseLength];
            await pipes.ResponsePipe.ReadExactlyAsync(responseBytes, 0, responseLength);

            return responseBytes;

        }
        catch (Exception ex) { return ex; }
    }

    /// <inheritdoc /> 
    public static async Task<Result> DisconnectAsync(ClientPipes pipes)
    {
        try
        {
            pipes.RequestPipe.Close();
            pipes.ResponsePipe.Close();

            return await Task.FromResult(Result.Success);
        }
        catch (Exception ex)
        {
            return await Task.FromResult(ex);
        }

    }

    #region private

    private static async Task<Result<ClientPipes>> ConnectAsync(string requestPipeName, string responsePipeName)
    {
        try
        {
            const int maxAttempts = 5;
            var delay = TimeSpan.FromMilliseconds(300);

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var reqPipe = new NamedPipeClientStream(".", requestPipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                    var resPipe = new NamedPipeClientStream(".", responsePipeName, PipeDirection.In, PipeOptions.Asynchronous);
                    await reqPipe.ConnectAsync(2_000);
                    await resPipe.ConnectAsync(2_000);
                    return new ClientPipes { RequestPipe = reqPipe, ResponsePipe = resPipe };
                }
                catch (Exception ex) when (ex is TimeoutException or IOException)
                {
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(delay);
                        delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 1.5, 2_000));
                    }
                    else
                        return Result.Error($"Failed to connect to pipes <{requestPipeName}> and/or <{responsePipeName}>", ex);
                }
            }
            return Result.Error($"Failed to connect to pipes <{requestPipeName}> and/or <{responsePipeName}>");
        }
        catch (Exception ex) { return ex; }
    }

    #endregion
}