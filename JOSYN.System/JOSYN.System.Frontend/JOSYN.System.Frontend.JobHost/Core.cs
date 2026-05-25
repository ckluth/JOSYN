using System.Text;
using JOSYN.Foundation.PropertyBag;
using JOSYN.Foundation.ResultPattern;
using JOSYN.System.Shared.Contract;
using JOSYN.System.Shared.Log;

namespace JOSYN.System.Frontend.JobHost;

/// <inheritdoc cref="ICore"/>
public sealed class Core : ICore
{
    /// <inheritdoc/>
    public static async Task<int> Run(string[] args)
    {
        Console.InputEncoding = new UTF8Encoding();
        Console.OutputEncoding = new UTF8Encoding();
#if DEBUG
        LocalLog.EnableConsoleOutput = true;
#endif

        try
        {
            var createJAPClient = await JAPClient.CreateConnectedClient(args);
            if (!createJAPClient.Succeeded)
            {
                LocalLog.Error(createJAPClient.ToResult());
                return -1;
            }

            var invokeResult = await JobInvoker.InvokeJob(createJAPClient.Value);
            if (invokeResult.Succeeded) return 0;

            LocalLog.Error(invokeResult);
            await ReportErrorToServer(createJAPClient.Value, invokeResult);
            return -2;
        }
        finally
        {
#if DEBUG
            Console.Write("\n[PRESS ANY KEY TO EXIT...]");
            Console.ReadKey(true);
#endif
        }
    }

    // -------------------------------------------------------------------------

    private static async Task ReportErrorToServer(IJosynApplicationProtocol client, Result error)
    {
        var report = new ErrorReport(
            error.ErrorMessage ?? string.Empty,
            error.CallStackAsString,
            error.Exception?.ToString(),
            DateTimeOffset.Now);

        var serialized = PropertyBag.Serialize(report, IniDictionarySerializer.Serialize);
        if (!serialized.Succeeded)
        {
            LocalLog.Error($"ErrorReport konnte nicht serialisiert werden: {serialized.ErrorMessage}");
            return;
        }

        var put = await client.PutError(serialized.Value);
        if (!put.Succeeded)
            LocalLog.Error($"PutError an Server fehlgeschlagen: {put.ErrorMessage}");
    }
}

