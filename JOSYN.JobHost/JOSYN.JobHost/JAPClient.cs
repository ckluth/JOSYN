using JOSYN.Foundation.JIP;
using JOSYN.Foundation.ResultPattern;
using JOSYN.System.Contract;

namespace JOSYN.JobHost;

internal sealed class JAPClient : IJosynApplicationProtocol
{
    private JAPClient() { }
    
    internal required ClientPipes Pipes { get; init; }
    
    internal static async Task<Result<JAPClient>> CreateConnectedClient(string[] args)
    {
        var sessionKey = PipesProtocol.ParseSessionKeyCLIArguments(args);
        if (sessionKey == Guid.Empty)
            return Result.Error("Der Anwendung wurde kein Pipes-SessionKey übergeben");
        
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