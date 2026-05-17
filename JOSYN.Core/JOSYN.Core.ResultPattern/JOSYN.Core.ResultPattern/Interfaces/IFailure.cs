#pragma warning disable IDE0130
namespace JOSYN.Core.ResultPattern;
#pragma warning restore IDE0130

/// <summary>
/// TODO
/// </summary>
public interface IError<out TSelf> where TSelf : IError<TSelf>
{
    /// <summary>
    /// TODO
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    /// TODO
    /// </summary>
    Exception? Exception { get; }

    /// <summary>
    /// TODO
    /// </summary>
    static abstract implicit operator TSelf(string error);

    /// <summary>
    /// TODO
    /// </summary>
    static abstract implicit operator TSelf(Exception exception);
}