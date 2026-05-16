#pragma warning disable IDE0130
namespace JOSYN.Core.ResultPattern;
#pragma warning restore IDE0130

public readonly record struct Failure(string ErrorMessage, Exception? Exception = null) : IFailure<Failure>
{
    public static implicit operator Failure(string error) => new(error);

    public static implicit operator Failure(Exception exception) => new(exception.Message, exception);
}