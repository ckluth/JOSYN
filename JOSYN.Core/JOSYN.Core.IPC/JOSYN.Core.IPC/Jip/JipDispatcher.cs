
#pragma warning disable IDE0130
namespace JOSYN.Core.IPC.JIP;
#pragma warning restore IDE0130

using System.Reflection;
using JOSYN.Core.ResultPattern;

/// <summary>
/// Routes incoming JIP requests by <see cref="Request.What"/> to registered handlers,
/// eliminating manual switch statements on the server side.
/// </summary>
/// <remarks>
/// Store the configured instance in a <c>static readonly</c> field and pass
/// <c>dispatcher.Dispatch</c> as <see cref="ServerStartArguments.HandleStringRequest"/>.
/// </remarks>
public sealed class JipDispatcher
{
    private readonly Dictionary<string, Func<string?, Task<Result<string?>>>> _handlers = new();
    private readonly Func<string, Task<string>> _builtDispatch;

    /// <summary>
    /// Returns the set of keys that have been registered so far.
    /// Intended for test assertions that verify protocol completeness.
    /// </summary>
    public IReadOnlySet<string> RegisteredKeys => _handlers.Keys.ToHashSet();

    /// <summary>Initializes a new <see cref="JipDispatcher"/> with no handlers registered.</summary>
    public JipDispatcher()
    {
        // Closure captures the _handlers reference — entries added via Register() are visible at dispatch time.
        _builtDispatch = JipServer.WrapHandler(async req =>
        {
            if (!_handlers.TryGetValue(req.What, out var handler))
                return Result<string?>.Fail($"Unbekannte Funktion: '{req.What}'");
            return await handler(req.Data);
        });
    }

    /// <summary>
    /// Registers an asynchronous handler that takes no input data and returns a string result.
    /// Suitable for methods like <c>Task&lt;Result&lt;string&gt;&gt; GetXxx()</c>.
    /// </summary>
    public JipDispatcher Register(string key, Func<Task<Result<string>>> handler)
        => RegisterCore(key, async _ =>
        {
            var r = await handler();
            return r.Succeeded ? Result<string?>.Success(r.Value) : r.ToResult<string?>();
        });

    /// <summary>
    /// Registers an asynchronous handler that takes a required string argument and returns void.
    /// Returns an error response if the request carries no data.
    /// Suitable for methods like <c>Task&lt;Result&gt; PutXxx(string value)</c>.
    /// </summary>
    public JipDispatcher Register(string key, Func<string, Task<Result>> handler)
        => RegisterCore(key, async data =>
        {
            if (data is null)
                return Result<string?>.Fail($"Funktion '{key}' erwartet Eingabedaten, erhielt aber null.");
            var r = await handler(data);
            return r.Succeeded ? Result<string?>.Success(null) : r.ToResult<string?>();
        });

    /// <summary>
    /// Registers an asynchronous handler that takes optional input data and returns an optional string.
    /// Suitable for the most general async handler shape.
    /// </summary>
    public JipDispatcher Register(string key, Func<string?, Task<Result<string?>>> handler)
        => RegisterCore(key, handler);

    /// <summary>
    /// Registers a synchronous handler that may use the request data and returns an optional string.
    /// Suitable for inline functions like <c>(data) => Result&lt;string?&gt;.Success("ECHO " + data)</c>.
    /// </summary>
    public JipDispatcher Register(string key, Func<string?, Result<string?>> handler)
        => RegisterCore(key, data => Task.FromResult(handler(data)));

    /// <summary>
    /// Registers a constant result — useful for fixed responses like PING or hard-wired config.
    /// </summary>
    public JipDispatcher Register(string key, Result<string?> constantResult)
        => RegisterCore(key, _ => Task.FromResult(constantResult));

    /// <summary>
    /// Registers all methods of <typeparamref name="TProtocol"/> by convention:
    /// the method name becomes the <c>What</c> key, the method signature determines the handler shape.
    /// </summary>
    /// <remarks>
    /// Supported signatures:
    /// <list type="bullet">
    /// <item><c>Task&lt;Result&lt;string&gt;&gt; Method()</c></item>
    /// <item><c>Task&lt;Result&gt; Method(string data)</c></item>
    /// </list>
    /// Throws <see cref="InvalidOperationException"/> at registration time for unsupported signatures.
    /// </remarks>
    /// <typeparam name="TProtocol">
    /// The protocol interface type. Always specify the interface explicitly, not the concrete class,
    /// to avoid registering non-protocol members.
    /// </typeparam>
    public JipDispatcher RegisterAll<TProtocol>(TProtocol impl) where TProtocol : class
    {
        foreach (var m in typeof(TProtocol).GetMethods())
        {
            var key = m.Name;
            var parameters = m.GetParameters();

            if (parameters.Length == 0 && m.ReturnType == typeof(Task<Result<string>>))
            {
                RegisterCore(key, async _ =>
                {
                    var r = await (Task<Result<string>>)m.Invoke(impl, null)!;
                    return r.Succeeded ? Result<string?>.Success(r.Value) : r.ToResult<string?>();
                });
            }
            else if (parameters.Length == 1
                     && parameters[0].ParameterType == typeof(string)
                     && m.ReturnType == typeof(Task<Result>))
            {
                RegisterCore(key, async data =>
                {
                    if (data is null)
                        return Result<string?>.Fail($"Funktion '{key}' erwartet Eingabedaten, erhielt aber null.");
                    var r = await (Task<Result>)m.Invoke(impl, [data])!;
                    return r.Succeeded ? Result<string?>.Success(null) : r.ToResult<string?>();
                });
            }
            else
            {
                throw new InvalidOperationException(
                    $"Methode '{typeof(TProtocol).Name}.{key}' hat eine nicht unterstützte Signatur für RegisterAll. " +
                    $"Unterstützt: Task<Result<string>> Method() oder Task<Result> Method(string).");
            }
        }
        return this;
    }

    /// <summary>
    /// Dispatches a raw JIP request string. Unknown <c>What</c> values are answered with an error response.
    /// Pass this method as <see cref="ServerStartArguments.HandleStringRequest"/>.
    /// </summary>
    public Task<string> Dispatch(string requestStr) => _builtDispatch(requestStr);

    private JipDispatcher RegisterCore(string key, Func<string?, Task<Result<string?>>> handler)
    {
        _handlers[key] = handler;
        return this;
    }
}
