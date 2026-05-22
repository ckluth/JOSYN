using JOSYN.Core.IPC;
using JOSYN.Core.IPC.JIP;
using JOSYN.Core.ResultPattern;

namespace JOSYN.JobHost;

/// <remarks>
/// Mal zur Abwechslung eine Klasse, die nicht statisch ist.
/// Wir haben hier einen echten "State", den wir über die Lifetime des Jobs halten müssen: die Pipes-Verbindung zum Host.
/// </remarks>
internal class JAPClient
{
    private JAPClient() { }
    internal required ClientPipes Pipes { get; init; }
    
    internal static async Task<Result<JAPClient>> CreateConnectedClient(string[] args)
    {
        var sessionKey = PipesProtocol.ParseSessionKeyCLIArguments(args);
        if (sessionKey == Guid.Empty)
            return Result.Error("Es wurde kein Pipes-SessionKey übergeben");
        
        var getPipes = await PipesClient.ConnectAsync(sessionKey);
        if (!getPipes.Succeeded)
            return Result<JAPClient>.Propagate(getPipes.ToResult<JAPClient>());

        var client = new JAPClient { Pipes = getPipes.Value };
        return client;
    }

    internal async Task<Result<string>> GetRawArguments()
    {
        var getConfig = await JipClient.SendAsync(Pipes, "GET-ARGUMENTS");  
        if (!getConfig.Succeeded)
            return Result<string>.Propagate(getConfig.ToResult<string>());
        return getConfig.Value;
    }
}