#pragma warning disable IDE0130
namespace JOSYN.Core.ResultPattern;
#pragma warning restore IDE0130

public interface IFailure<out TSelf> where TSelf : IFailure<TSelf>
{
    string ErrorMessage { get; }
    
    Exception? Exception { get; }
    
    static abstract implicit operator TSelf(string error);

    static abstract implicit operator TSelf(Exception exception);
}