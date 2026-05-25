using System.Text;

namespace JOSYN.System.Frontend.JobHost;

/// <inheritdoc cref="ICore"/>
public sealed class Core : ICore
{
    /// <inheritdoc/>
    public static async Task<int> Run(string[] args)
    {
        Console.InputEncoding = new UTF8Encoding();
        Console.OutputEncoding = new UTF8Encoding();

        try
        {
            var createJAPClient = await JAPClient.CreateConnectedClient(args);
            if (!createJAPClient.Succeeded)
            {
                FakeCore.LogLocalError(createJAPClient.ToResult());
                return -1;
            }
            
            var invokeResult = await JobInvoker.InvokeJob(createJAPClient.Value);
            if (invokeResult.Succeeded) return 0;

            FakeCore.LogError(invokeResult);
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
}

