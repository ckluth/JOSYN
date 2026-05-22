using JOSYN.Core.IPC;
using System.Text;

namespace JOSYN.JobHost;

public class Core : ICore
{
    public static async Task<int> Run(string[] args)
    {
        Console.InputEncoding = new UTF8Encoding();
        Console.OutputEncoding = new UTF8Encoding();
        
        var createJAPClient = await JAPClient.CreateConnectedClient(args);
        if (!createJAPClient.Succeeded || createJAPClient.Value == null)
        {
            FakeCore.LogLocalError(createJAPClient.ToResult());
            return -1;
        }
        var invokeResult = await JobRunner.InvokeJob(createJAPClient.Value);

        if (invokeResult.Succeeded) return 0;
        
        FakeCore.LogError(invokeResult.ToResult());
        return -2;
    }
}

