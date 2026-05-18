using JOSYN.Core.ResultPattern;

namespace JOSYN.Core.IPC;

/// <summary>
/// TODO
/// </summary>
public class PipesProtocol: IPipesProtocol
{
    /// <summary>
    /// TODO
    /// </summary>
    public const string MagicToken = "JOSYN-IPC";

    /// <inheritdoc/>
    public static string CreateClientStartCLIArguments(string sessionKey)
    {
        return $"{PipesProtocol.MagicToken} {sessionKey}";
    }

    /// <inheritdoc/>
    public static Result<(string sessionKey, string clientExePath)> ParseServerCLIArguments(string[] args)
    {
        if (args.Length is < 2 or > 3 || !args[0].Equals(PipesProtocol.MagicToken))
            return Result.Error("Invalid IPC-Arguments");

        return args.Length == 2 ? (args[1], string.Empty) : (args[1], args[2]);
    }

    /// <inheritdoc/>
    public static string? ParseSessionKeyFromCLIArguments(string[] args)
    {
        return args is not [PipesProtocol.MagicToken, _] ? null : args[1];
    }

    /// <inheritdoc/>
    public static (string requestPipeName, string responsePipeName) DerivePipeNamesFromSessionKey(string sessionKey)
    {
        var requestPipeName = $"req-pipe-{sessionKey}";
        var responsePipeName = $"res-pipe-{sessionKey}";
        return (requestPipeName, responsePipeName);
    }
}