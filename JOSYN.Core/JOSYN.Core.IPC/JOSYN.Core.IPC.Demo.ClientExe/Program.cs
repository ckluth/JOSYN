using JOSYN.Core.ResultPattern;
using System.Text;

#pragma warning disable IDE0130
namespace JOSYN.Core.IPC.Demo;
#pragma warning restore IDE0130

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var sessionKey = PipesProtocol.ParseSessionKeyFromClientCLIArguments(args);
        
        if (string.IsNullOrEmpty(sessionKey))
            return LogError("Es wurde kein Pipes-SessionKey übergeben", 1);

        Console.WriteLine("SessionKey: " + sessionKey);

        var getPipes = await PipesClient.ConnectAsync(sessionKey);
        if (!getPipes.Succeeded)
            return LogErrorResult(getPipes.ToResult(), 1);

        var pipes = getPipes.Value;
        
        var getResponse = await PipesClient.SendRequestAsync("GET-SOMETHING", pipes);
        if (!getResponse.Succeeded)
            return LogErrorResult(getResponse.ToResult(), 1);
        
        Console.WriteLine(getResponse.Value);
        Console.ReadKey();
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