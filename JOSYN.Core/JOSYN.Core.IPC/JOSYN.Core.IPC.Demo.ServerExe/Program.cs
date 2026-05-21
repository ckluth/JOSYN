using JOSYN.Core.IPC.JIP;
using JOSYN.Core.ResultPattern;
using System.Diagnostics;
using System.Text;

namespace JOSYN.Core.IPC.Demo.ServerExe;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            Console.WriteLine("ARGS: " + string.Join(" | ", args));
            var sessionKey = PipesProtocol.ParseSessionKeyCLIArguments(args);
            if (sessionKey == Guid.Empty)
                return LogError("Keine IPC-Session-UID angegeben.", 1);
            
            return await RunServer(sessionKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return 1;
        }
    }

    private static async Task<int> RunServer(Guid sessionKey, int? timeout = null)
    {
        Console.WriteLine("Starting Server...");
        var sw = Stopwatch.StartNew();

        var serverStartArguments = new ServerStartArguments
        {
            ConnectionTimeout = TimeSpan.FromDays(timeout ?? 1),
            HandleStringRequest = HandleRequest,
            SessionKey = sessionKey,
            HandleErrorNotification = HandleHandlerError,
            IsCancellationRequested = WasEscapePressed,
        };
        
        var res = await PipesServer.RunAsync(serverStartArguments, true, () =>
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nResestablishing Connection\n");
            Console.ResetColor();
        } );

        Console.WriteLine($"Finished after {sw.Elapsed}");
        return !res.Succeeded ? LogErrorResult(res, 1) : TerminateWithSuccess(); ;
    }

    private static Task<bool> WasEscapePressed()
    {
        if (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
            return Task.FromResult(false);
        Console.WriteLine("ESC gedrückt. Abbruch...");
        
        return Task.FromResult(true);
    }

    private static readonly Func<string, Task<string>> _dispatch = JipServer.WrapHandler(req => req.What switch
    {
        "PING"       => JipProtocol.ToResponse(Result.Success),
        "GET-CONFIG" => JipProtocol.ToResponse(Result<string>.Success("{ \"version\": \"1.0\", \"mode\": \"demo\" }"), d => d),
        "GET-DICT"   => new Response { Status = ResponseStatus.Success, Dict = new Dictionary<string, string> { ["host"] = "localhost", ["port"] = "5000" } },
        _            => JipProtocol.ToLogicalFailureResponse($"Unbekannte Funktion: '{req.What}'"),
    });

    private static async Task<string> HandleRequest(string requestStr)
    {
        Console.WriteLine($"SRV|RECEIVED> {requestStr}");
        var responseStr = await _dispatch(requestStr);
        Console.WriteLine($"SRV|SENDING>  {responseStr}");
        return responseStr;
    }

    #region FakeLog
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

