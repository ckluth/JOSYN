using JOSYN.Core.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Core.IPC;
#pragma warning restore IDE0130

/// <summary>
/// TODO
/// </summary>
public interface IPipesClient
{
    /// <summary>
    /// TODO
    /// </summary>
    static abstract Task<Result<ClientPipes>> ConnectAsync(string sessionKey);

    /// <summary>
    /// TODO
    /// </summary>
    static abstract Task<Result<byte[]>> SendRequestAsync(byte[] requestBytes, ClientPipes pipes);

    /// <summary>
    /// TODO
    /// </summary>
    static abstract Task<Result<string>> SendRequestAsync(string request, ClientPipes pipes);

    /// <summary>
    /// TODO
    /// </summary>
    static abstract Task<Result> DisconnectAsync(ClientPipes pipes);
}