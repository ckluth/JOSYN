using JOSYN.Core.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Core.IPC;
#pragma warning restore IDE0130

/// <summary>
/// TODO
/// </summary>
public interface IPipesProtocol
{
    /// <summary>
    /// TODO
    /// </summary>
    public const string MagicToken = "JOSYN-IPC";

    /// <summary>
    /// TODO
    /// </summary>
    public const string MagicErrorToken = $"{MagicToken}-ERROR";

    /// <summary>
    /// TODO
    /// </summary>
    public const string MagicBusyToken = $"{MagicErrorToken}-BUSY";

    /// <summary>
    /// TODO
    /// </summary>
    public const string MagicShutdownToken = $"{MagicToken}-SHUTDOWN";

    /// <summary>
    /// TODO
    /// </summary>
    static abstract string CreateClientStartCLIArguments(string sessionKey);

    /// <summary>
    /// TODO
    /// </summary>        
    static abstract Guid ParseSessionKeyCLIArguments(string[] args);

    /// <summary>
    /// TODO
    /// </summary>            
    static abstract (string requestPipeName, string responsePipeName) DerivePipeNamesFromSessionKey(string sessionKey);
}