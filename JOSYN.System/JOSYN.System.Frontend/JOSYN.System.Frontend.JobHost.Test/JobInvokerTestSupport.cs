using JOSYN.Foundation.ResultPattern;
using JOSYN.System.Shared.Contract;

namespace JOSYN.System.Frontend.JobHost.Test;

// ── Fake protocol ─────────────────────────────────────────────────────────────

internal sealed class FakeProtocol(string rawArguments = "") : IJosynApplicationProtocol
{
    public string? LastPutResult { get; private set; }

    public Task<Result<string>> GetRawArguments()
        => Task.FromResult(Result<string>.Success(rawArguments));

    public Task<Result> PutRawResult(string result)
    {
        LastPutResult = result;
        return Task.FromResult(Result.Success);
    }

    public Task<Result> PutError(string serializedError)
        => Task.FromResult(Result.Success);
}

internal sealed class FailingGetArgumentsProtocol : IJosynApplicationProtocol
{
    public Task<Result<string>> GetRawArguments()
        => Task.FromResult(Result<string>.Fail("Verbindung verloren"));

    public Task<Result> PutRawResult(string result) => Task.FromResult(Result.Success);
    public Task<Result> PutError(string serializedError) => Task.FromResult(Result.Success);
}
