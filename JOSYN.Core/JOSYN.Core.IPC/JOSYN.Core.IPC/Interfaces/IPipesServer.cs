using JOSYN.Core.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Core.IPC;
#pragma warning restore IDE0130

/// <summary>
/// TODO
/// </summary>
public interface IPipesServer
{
    /// <summary>
    /// TODO
    /// </summary>
    static abstract Task<Result> RunAsync(string clientExePath, Func<string, string> processRequest, TimeSpan connectTimeout, string? sessionKey = null, Func<bool>? shouldCancel = null);

    /// <summary>
    /// TODO
    /// </summary>
    static abstract Task<Result> RunAsync(string clientExePath, Func<byte[], byte[]> processRequest, TimeSpan connectTimeout, string? sessionKey = null, Func<bool>? shouldCancel = null);
    
    /// <summary>
    /// TODO
    /// </summary>
    static abstract Task<Result> RunAsync(Func<string, string> processRequest, TimeSpan connectTimeout, string sessionKey, Func<bool>? shouldCancel = null);

    /// <summary>
    /// TODO
    /// </summary>
    static abstract Task<Result> RunAsync(Func<byte[], byte[]> processRequest, TimeSpan connectTimeout, string sessionKey, Func<bool>? shouldCancel = null);

    /// <summary>
    /// TODO
    /// </summary>
    static abstract Task<Result> RunAsync(Func<byte[], byte[]> processRequest, TimeSpan connectTimeout, string sessionKey, CancellationToken cancellationToken = default);

}