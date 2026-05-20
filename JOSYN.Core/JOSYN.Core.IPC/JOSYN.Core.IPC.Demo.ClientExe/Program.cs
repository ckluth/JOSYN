using JOSYN.Core.IPC.JIP;
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

        Console.WriteLine("Connected.\n");
        var pipes = getPipes.Value;

        // --- PING (void-Ergebnis) ---
        if (await SendAndPrint(pipes, new Request { What = "PING" }, JipProtocol.ToResult) is not 0 and var e1) return e1;

        // --- GET-CONFIG (string-Ergebnis) ---
        if (await SendAndPrint(pipes, new Request { What = "GET-CONFIG" }, r => JipProtocol.ToResult<string>(r, d => d)) is not 0 and var e2) return e2;

        // --- GET-DICT (dict-Ergebnis, manuell ausgelesen) ---
        if (await SendAndPrint(pipes, new Request { What = "GET-DICT" }, JipProtocol.ToResult) is not 0 and var e3) return e3;

        // --- Unbekannte Funktion (LogicalFailure) ---
        await SendAndPrint(pipes, new Request { What = "DO-MAGIC" }, JipProtocol.ToResult);

        Console.WriteLine("\n[PRESS KEY TO DISCONNECT]");
        Console.ReadKey(true);

        await PipesClient.DisconnectAsync(pipes, sendShutdownRequest: false);
        Console.WriteLine("Disconnected.\n[PRESS KEY TO EXIT]");
        Console.ReadKey(true);
        return 0;
    }

    private static async Task<int> SendAndPrint<T>(
        ClientPipes pipes,
        Request request,
        Func<Response, T> toResult)
    {
        Console.WriteLine($"CLI|SENDING> {request}");

        var getRaw = await PipesClient.SendRequestAsync(request.ToString(), pipes);
        if (!getRaw.Succeeded)
            return LogErrorResult(getRaw.ToResult(), 1);

        var parseResult = JipProtocol.ParseResponse(getRaw.Value);
        if (!parseResult.Succeeded)
            return LogErrorResult(parseResult.ToResult(), 1);

        var response = parseResult.Value;
        Console.WriteLine($"CLI|STATUS>  {response.Status}");

        if (response.HasError)
            Console.WriteLine($"CLI|ERROR>   {response.Error}");
        else if (response.Data is not null)
            Console.WriteLine($"CLI|DATA>    {response.Data}");
        else if (response.Dict is not null)
            Console.WriteLine($"CLI|DICT>    {string.Join(", ", response.Dict.Select(kv => $"{kv.Key}={kv.Value}"))}");
        else
            Console.WriteLine("CLI|OK>      (kein Datenwert)");

        Console.WriteLine();
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