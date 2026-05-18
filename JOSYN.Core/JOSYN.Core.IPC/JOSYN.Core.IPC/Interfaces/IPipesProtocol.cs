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
    static abstract string CreateClientStartCLIArguments(string sessionKey);

    /// <summary>
    /// TODO
    /// </summary>    
    static abstract Result<(string sessionKey, string clientExePath)> ParseServerCLIArguments(string[] args);

    /// <summary>
    /// TODO
    /// </summary>        
    static abstract string? ParseSessionKeyFromCLIArguments(string[] args);

    /// <summary>
    /// TODO
    /// </summary>            
    static abstract (string requestPipeName, string responsePipeName) DerivePipeNamesFromSessionKey(string sessionKey);
}