using JOSYN.Core.ResultPattern;
using System.Text;

#pragma warning disable IDE0130
namespace JOSYN.Core.IPC.Demo;
#pragma warning restore IDE0130

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var sessionKey = PipesProtocol.ParseSessionKeyCLIArguments(args);
        
        if (sessionKey == Guid.Empty)
            return LogError("Es wurde kein Pipes-SessionKey übergeben", 1);
        
        
        Console.WriteLine("SessionKey: " + sessionKey);

        Console.WriteLine("[PRESS KEY TO CONNECT]");
        Console.ReadKey(true);

        var getPipes = await PipesClient.ConnectAsync(sessionKey);
        if (!getPipes.Succeeded)
            return LogErrorResult(getPipes.ToResult(), 1);

        Console.WriteLine("Connected.");

        var pipes = getPipes.Value;

        Console.WriteLine("\nSending Request...");
        
        const string myRequest = "GET-CONFIG";
        Console.WriteLine($"CLI|SENDING>{myRequest}");
        var getResponse = await PipesClient.SendRequestAsync(myRequest, pipes);
        if (!getResponse.Succeeded)
            return LogErrorResult(getResponse.ToResult(), 1);
        Console.WriteLine( $"CLI|RECEIVED>{getResponse.Value}");
        
        Console.WriteLine("\nStill Connected.");
        Console.WriteLine("[PRESS KEY TO DISCONNECT]");
        Console.ReadKey(true);

        await PipesClient.DisconnectAsync(pipes, sendShutdownRequest: true);
        
        Console.WriteLine("\nDisconnected.");
        Console.WriteLine("[PRESS KEY TO EXIT]");
        Console.ReadKey(true);

        return 0;
    }

    #region FakeLog

    private static int LogErrorResult(Result result, int exitCode)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[Errormessage]");
        sb.AppendLine(result.ErrorMessage);
        sb.AppendLine();
        sb.AppendLine("[Callstack]");
        sb.AppendLine(result.CallStackAsString);
        if (result.Exception != null)
        {
            sb.AppendLine();
            sb.AppendLine("[Exception]");
            sb.AppendLine(result.Exception.ToString());
        }
        return LogError(sb.ToString(), exitCode);
    }

    private static int LogError(string msg, int exitCode)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ResetColor();
        Console.WriteLine("\n[PRESS ANY KEY]");
        Console.ReadKey();
        return exitCode;
    }

    #endregion    
}