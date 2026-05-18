using System.Diagnostics;
using System.Text;
using JOSYN.Core.ResultPattern;

namespace JOSYN.Core.IPC.Demo.ServerExe;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.WriteLine("ARGS: " + string.Join(" | ", args));
        var parseArgs = PipesProtocol.ParseServerCLIArguments(args);
        if (!parseArgs.Succeeded)
            return LogErrorResult(parseArgs.ToResult(), 1, "Keine IPC-Argumente angegeben. ");
        
        var arguments = parseArgs.Value;

        if (string.IsNullOrEmpty(arguments.clientExePath))
            return await RunSzenario02(arguments.sessionKey, 10);
        
        
        return await RunSzenario03(arguments.clientExePath, arguments.sessionKey);

        // Specified Szenario 1 not implemented in this Demo so far...
    }
    
    private static async Task<int> RunSzenario02(string sessionKey, int? timeout = null)
    {
        // No Client-Exe start; wait for Any Client, which must know the session-key (a 3rd Party must have started Server and Client with sessionKEy as CLI-Argument)

        // Wait for started Client => Single Connection-Attempt-Loop with specified Timeout or user-cancellation
        // RequestLoop with User-Cancellation (by ESC-Key) 
        // Terminate silently when Client closes Connection (explicitly or just by termination)

        Console.WriteLine("No client executable path provided. Just listening until specified timeout and/or user-cancellation...");
        var sw = Stopwatch.StartNew();
        
        var mayBeInfiniteTimeout = TimeSpan.FromSeconds(timeout ?? int.MaxValue);
        var res = await PipesServer.RunAsync(HandleRequest, mayBeInfiniteTimeout, sessionKey, WasEscapePressed);
        
        Console.WriteLine($"Finished after {sw.Elapsed}");
        return !res.Succeeded ? LogErrorResult(res, 1) : 0;
    }

    private static async Task<int> RunSzenario03(string clientExePath, string sessionKey, int? timeout = null)
    {
        // Client-Exe will get startet by Process.Run() with sessionKEy as CLI-Argument

        // Wait for started Client => Single Connection-Attempt-Loop with specified Timeout
        // RequestLoop with User-Cancellation (by ESC-Key) 
        // Terminate silently when Client closes Connection (explicitly or just by termination)
        
        Console.WriteLine($"Starting {Path.GetFileName(clientExePath)}");
        
        var sw = Stopwatch.StartNew();

        var mayBeInfiniteTimeout = TimeSpan.FromSeconds(timeout ?? int.MaxValue);
        var res = await PipesServer.RunAsync(clientExePath, HandleRequest, mayBeInfiniteTimeout, sessionKey, WasEscapePressed);

        Console.WriteLine($"Finished after {sw.Elapsed}");
        return !res.Succeeded ? LogErrorResult(res, 1) : 0;
    }

    private static bool WasEscapePressed()
    {
        if (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
            return false;
        Console.WriteLine("ESC gedrückt. Abbruch...");
        return true;
    }

    private static Task<string> HandleRequest(string requestStr)
    {
        Console.WriteLine($"SRV|RECEIVED>{requestStr}");
        var responseStr = $"Echo: {requestStr}";
        Console.WriteLine($"SRV|SENDING>{responseStr}");
        return Task.FromResult(responseStr);
    }

    #region FakeLog

    private static int LogErrorResult(Result result, int exitCode, string? msg = null)
    {
        if (!string.IsNullOrEmpty(msg))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(msg);
            Console.ResetColor();
            Console.WriteLine();
        }

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

