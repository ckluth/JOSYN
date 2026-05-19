using System.Diagnostics;
using System.Text;
using JOSYN.Core.ResultPattern;

namespace JOSYN.Core.IPC.Demo.ServerExe;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {

            Console.WriteLine("ARGS: " + string.Join(" | ", args));
            var parseArgs = PipesProtocol.ParseServerCLIArguments(args);
            if (!parseArgs.Succeeded)
                return LogErrorResult(parseArgs.ToResult(), 1, "Keine IPC-Argumente angegeben. ");

            var arguments = parseArgs.Value;

            //if (string.IsNullOrEmpty(arguments.clientExePath))
            return await RunSzenario02(arguments.sessionKey);

            //return await RunSzenario03(arguments.clientExePath, arguments.sessionKey);

            // Specified Szenario 1 not implemented in this Demo so far...

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return 1;
        }
    }

    private static async Task<int> RunSzenario02(string sessionKey, int? timeout = null)
    {
        // No Client-Exe start; wait for Any Client, which must know the session-key (a 3rd Party must have started Server and Client with sessionKEy as CLI-Argument)

        // Wait for started Client => Single Connection-Attempt-Loop with specified Timeout or user-cancellation
        // RequestLoop with User-Cancellation (by ESC-Key) 
        // Terminate silently when Client closes Connection (explicitly or just by termination)

        Console.WriteLine("No client executable path provided. Just listening until specified timeout and/or user-cancellation...");
        var sw = Stopwatch.StartNew();

        var mayBeInfiniteTimeout = TimeSpan.FromDays(timeout ?? 1);

        Result res;
        while (true)
        {
            res = await PipesServer.RunAsync(HandleRequest, mayBeInfiniteTimeout, HandleHandlerError, sessionKey, WasEscapePressed);
            if (res.Succeeded)
            {
                if (wasEscaped)
                    break;
                
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\nResestablishing Connection\n");
                Console.ResetColor();
            } else
                break;
        }

        Console.WriteLine($"Finished after {sw.Elapsed}");

        return !res.Succeeded ? LogErrorResult(res, 1) : TerminateWithSuccess(); ;
    }
  


    private static async Task<int> RunSzenario03(string clientExePath, string sessionKey, int? timeout = null)
    {
        // Client-Exe will get startet by Process.Run() with sessionKEy as CLI-Argument

        // Wait for started Client => Single Connection-Attempt-Loop with specified Timeout
        // RequestLoop with User-Cancellation (by ESC-Key) 
        // Terminate silently when Client closes Connection (explicitly or just by termination)

        Console.WriteLine($"Starting {Path.GetFileName(clientExePath)}");

        var sw = Stopwatch.StartNew();

        var mayBeInfiniteTimeout = TimeSpan.FromDays(timeout ?? 1);
        var res = await PipesServer.RunAsync(clientExePath, HandleRequest, mayBeInfiniteTimeout, HandleHandlerError, sessionKey, WasEscapePressed);

        Console.WriteLine($"Finished after {sw.Elapsed}");
        return !res.Succeeded ? LogErrorResult(res, 1) : TerminateWithSuccess();
    }

    private static async Task HandleHandlerError(string request, Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine();
        Console.WriteLine($"Error handling request: {request}");
        Console.WriteLine($"Exception: {ex}");
        Console.WriteLine();
        Console.ResetColor();
        await Task.CompletedTask;
    }



    private static bool wasEscaped = false;

    private static Task<bool> WasEscapePressed()
    {
        throw new Exception("aaa");
        if (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
            return Task.FromResult(false);
        Console.WriteLine("ESC gedrückt. Abbruch...");
        wasEscaped = true;
        return Task.FromResult(true);
    }

    private static Task<string> HandleRequest(string requestStr)
    {
        Console.WriteLine($"SRV|RECEIVED>{requestStr}");
        var responseStr = $"Echo: {requestStr}";
        Console.WriteLine($"SRV|SENDING>{responseStr}");
        throw new Exception("Oh No!");
        return Task.FromResult(responseStr);
    }

    #region FakeLog

    private static int TerminateWithSuccess()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Terminated...");
        Console.ResetColor();
        Console.WriteLine("\n[PRESS ANY KEY]");
        Console.ReadKey();
        return 0;
    }

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

    private static int LogError(string msg, int exitCode, bool waitForKeyPress = true)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ResetColor();

        if (!waitForKeyPress) return 0;
        
        Console.WriteLine("\n[PRESS ANY KEY]");
        Console.ReadKey();
        return exitCode;

    }

    #endregion

}

