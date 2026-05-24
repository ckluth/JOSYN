using JOSYN.Foundation.JIP;
using JOSYN.Foundation.ResultPattern;
using JOSYN.System.Contract;
using System.Diagnostics;
using System.Text;

namespace JOSYN.System.Backend.JAPServer;

internal static class Backend
{
    internal static async Task<int> Run(string[] args)
    {
        Console.InputEncoding  = new UTF8Encoding();
        Console.OutputEncoding = new UTF8Encoding();

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
        finally
        {
#if DEBUG
            Console.WriteLine("\n[PRESS ANY KEY]\n");
            Console.ReadKey(true);
#endif
        }
    }

    // -------------------------------------------------------------------------
    // Server lifecycle
    // -------------------------------------------------------------------------

    private static async Task<int> RunServer(Guid sessionKey)
    {
        Console.WriteLine("Starting Server...");
        var sw = Stopwatch.StartNew();

        var serverStartArguments = new ServerStartArguments
        {
            ConnectionTimeout    = TimeSpan.FromDays(1),
            HandleStringRequest  = HandleRequest,
            SessionKey           = sessionKey,
            HandleErrorNotification = HandleHandlerError,
            IsCancellationRequested = WasEscapePressed,
        };

        var res = await PipesServer.RunAsync(serverStartArguments, true, () =>
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nResestablishing Connection\n");
            Console.ResetColor();
        });

        Console.WriteLine($"Finished after {sw.Elapsed}");
        return res.Succeeded ? TerminateWithSuccess() : LogErrorResult(res, 1);
    }

    private static Task<bool> WasEscapePressed()
    {
        if (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
            return Task.FromResult(false);
        Console.WriteLine("ESC gedrückt. Abbruch...");
        return Task.FromResult(true);
    }

    // -------------------------------------------------------------------------
    // Dispatch
    // -------------------------------------------------------------------------

    private static readonly JAPServer _japServer = new();

    private static readonly JipDispatcher _jipDispatcher =
        new JipDispatcher().RegisterAll<IJosynApplicationProtocol>(_japServer);

    private static async Task<string> HandleRequest(string requestStr)
    {
        Console.WriteLine($"SRV|RECEIVED> {requestStr}");
        var responseStr = await _jipDispatcher.Dispatch(requestStr);
        Console.WriteLine($"SRV|SENDING>  {responseStr}");
        return responseStr;
    }

    private static async Task HandleHandlerError(string request, Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine();
        Console.WriteLine($"Fehler beim Verarbeiten der Anfrage: {request}");
        Console.WriteLine($"Exception: {ex}");
        Console.WriteLine();
        Console.ResetColor();
        await Task.CompletedTask;
    }

    // -------------------------------------------------------------------------
    // Console helpers
    // -------------------------------------------------------------------------

    private static int TerminateWithSuccess()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Terminated...");
        Console.ResetColor();
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
        sb.AppendLine("[Fehlermeldung]");
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
        return exitCode;
    }
}
