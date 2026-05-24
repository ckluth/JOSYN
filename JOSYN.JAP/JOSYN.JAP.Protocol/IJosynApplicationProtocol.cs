using JOSYN.Core.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.JAP;
#pragma warning restore IDE0130

/// <summary>
/// TODO
/// </summary>
public interface IJosynApplicationProtocol
{
    /// <summary>
    /// TODO
    /// </summary>
    /// <returns></returns>
    Task<Result<string>> GetRawArguments();

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    Task<Result> PutRawResult(string result);

}