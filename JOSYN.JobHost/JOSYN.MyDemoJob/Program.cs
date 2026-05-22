using System.Text;

namespace MyDemoJob;

internal class Program
{
    private static async Task<int> Main(string[] args) { return await JOSYN.JobHost.Core.Run(args); }
}

