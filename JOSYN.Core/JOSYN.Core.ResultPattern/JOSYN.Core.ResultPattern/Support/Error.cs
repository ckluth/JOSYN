#pragma warning disable IDE0130
namespace JOSYN.Core.ResultPattern;
#pragma warning restore IDE0130


/// inheritdoc />   
public readonly record struct Error(string ErrorMessage, Exception? Exception = null) : IError<Error>
{

    /// inheritdoc />   
    public static implicit operator Error(string error) => new(error);

    /// inheritdoc />   

    public static implicit operator Error(Exception exception) => new(exception.Message, exception);
}