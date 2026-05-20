namespace JOSYN.Core.IPC;

/// <summary>
/// 
/// </summary>
public interface IServerStartArguments
{
    /// <summary>
    /// 
    /// </summary>
    Guid SessionKey { get; init; }

    /// <summary>
    /// 
    /// </summary>
    string? ClientExePath { get; init; }

    /// <summary>
    /// 
    /// </summary>
    Func<string, Task<string>>? HandleStringRequest { get; init; }

    /// <summary>
    /// 
    /// </summary>
    Func<byte[], Task<byte[]>>? HandleRawRequest { get; init; }
    
    /// <summary>
    /// 
    /// </summary>
    bool HasStringRequestHandler { get; }

    /// <summary>
    /// 
    /// </summary>
    TimeSpan ConnectionTimeout { get; init; }

    /// <summary>
    /// 
    /// </summary>
    Func<string, Exception, Task> HandleErrorNotification { get; init; }

    /// <summary>
    /// TODO
    /// </summary>
    Func<Task<bool>>? IsCancellationRequested { get; init; }

}