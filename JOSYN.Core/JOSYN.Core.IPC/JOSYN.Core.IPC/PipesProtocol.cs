using JOSYN.Core.ResultPattern;

namespace JOSYN.Core.IPC;

/// <summary>
/// TODO
/// </summary>
public class PipesProtocol: IPipesProtocol
{
    /// <inheritdoc/>
    public static string CreateClientStartCLIArguments(string sessionKey)
    {
        return $"{IPipesProtocol.MagicToken} {sessionKey}";
    }

    /// <inheritdoc/>
    public static Guid ParseSessionKeyCLIArguments(string[] args)
    {
        if (args is not [IPipesProtocol.MagicToken, _])
            return Guid.Empty;

        return Guid.TryParse(args[1], out var guid) ? guid : Guid.Empty;
    }

    /// <inheritdoc/>
    public static (string requestPipeName, string responsePipeName) DerivePipeNamesFromSessionKey(string sessionKey)
    {
        var requestPipeName = $"req-pipe-{sessionKey}";
        var responsePipeName = $"res-pipe-{sessionKey}";
        return (requestPipeName, responsePipeName);
    }
}