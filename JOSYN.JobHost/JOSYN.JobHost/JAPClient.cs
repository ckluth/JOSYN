using JOSYN.Core.IPC;
using JOSYN.Core.IPC.JIP;
using JOSYN.Core.ResultPattern;

namespace JOSYN.JobHost;

// 
//  TODO: mit Server sharen
//
public interface IJosynApplicationProtocol
{
    Task<Result<string>> GetRawArguments();

    Task<Result> PutRawResult(string result);

}

/// <remarks>
/// Mal zur Abwechslung eine Klasse, die nicht statisch ist.
/// Wir haben hier einen echten "State", den wir über die Lifetime des Jobs halten müssen: die Pipes-Verbindung zum Host.
/// </remarks>
internal class JAPClient : IJosynApplicationProtocol
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

    async Task<Result<string>> IJosynApplicationProtocol.GetRawArguments()
    {
        var getConfig = await JipClient.SendAsync(Pipes, nameof(IJosynApplicationProtocol.GetRawArguments));
        
        if (!getConfig.Succeeded)
            return getConfig.ToResult<string>();
        
        return getConfig.Value ?? 
               Result<string>.Fail("Server lieferte keine Daten zurück.");
    }

    async Task<Result> IJosynApplicationProtocol.PutRawResult(string result)
    {
        var putJobResult = await JipClient.SendAsync(Pipes, nameof(IJosynApplicationProtocol.PutRawResult), result);
        
        return !putJobResult.Succeeded 
            ? Result.Propagate(putJobResult.ToResult()) 
            : Result.Success;
    }
}