using System.Text;
using JOSYN.Core.ResultPattern;

namespace JOSYN.Core.IPC.Demo.ServerExe;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var parseArgs = PipesProtocol.ParseServerCLIArguments(args);
        if (!parseArgs.Succeeded)
            return LogErrorResult(parseArgs.ToResult(), 1);
        
        var arguments = parseArgs.Value;
        if (string.IsNullOrEmpty(arguments.clientExePath))
            return LogError("Client executable path is missing.", 1);
        
        Console.WriteLine($"Starte {arguments.clientExePath}");
        var res = await PipesServer.RunAsync(arguments.clientExePath, ProcessRequest, TimeSpan.FromSeconds(10), arguments.sessionKey);
        return !res.Succeeded ? LogErrorResult(res, 1) : 0;
    }
    
    private static string ProcessRequest(string requestStr)
    {
        var responseStr = $"Echo2: {requestStr}";
        return responseStr;
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

