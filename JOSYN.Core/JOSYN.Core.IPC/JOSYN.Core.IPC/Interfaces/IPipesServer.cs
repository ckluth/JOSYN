using JOSYN.Core.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Core.IPC;
#pragma warning restore IDE0130

/// <summary>
/// TODO
/// </summary>
public interface IPipesServer
{
    /// <summary>
    /// 
    /// </summary>
    static abstract Task<Result> RunAsync(ServerStartArguments args, bool reConnect = false, Action? onReconnect= null);
}