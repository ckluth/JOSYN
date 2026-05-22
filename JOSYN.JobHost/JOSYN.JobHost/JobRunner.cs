using System.Reflection;
using System.Text;
using JOSYN.Core.PropertyBag;
using JOSYN.JobHost.Attributes;
using JOSYN.Core.ResultPattern;

namespace JOSYN.JobHost;

//public static class JobRunner<T> where T : class
//{
//    public static ArgumentsComparer<T>? ConditionalParallelExecutionAllowed { get; set; }
//}

internal static class JobRunner
{
    internal static async Task<Result<JobExectionResult>> InvokeJob(JAPClient japClient, Type? entrypointType = null)
    {
        try
        {
            //
            // Assembly laden
            // 
            var findEntrypointAssembly = FindEntryPointAssembly(entrypointType);
            if (!findEntrypointAssembly.Succeeded)
                return Result<JobExectionResult>.Propagate(findEntrypointAssembly.ToResult<JobExectionResult>());

            //
            // Einsprungs-Methode finden
            // 
            var findJobFunc = FindJobFunction(findEntrypointAssembly.Value);
            if (!findJobFunc.Succeeded)
                return Result<JobExectionResult>.Propagate(findJobFunc.ToResult<JobExectionResult>());

            //
            // Aufrufsargumente erzeugen
            //
            var getInvocationArgs = await CreateInvocationArguments(findJobFunc.Value, japClient);

            if (!getInvocationArgs.Succeeded)
                return Result<JobExectionResult>.Propagate(getInvocationArgs.ToResult<JobExectionResult>());
            var invocationArgs = getInvocationArgs.Value;

            //
            // Hier kommt endlich der Aufruf der Job-Methode
            //
            object? res = null;
            try
            {
                res = findJobFunc.Value.Invoke(null, invocationArgs);
            }
            catch (Exception ex) { return Result.Error("Der Job hat eine unbehandelte Exception durchgelassen.", ex); }

            //
            // Jetzt noch das Result verarbeiten
            //
            var resultType = findJobFunc.Value.ReturnType;
            var nullabilityInfo = new NullabilityInfoContext().Create(findJobFunc.Value.ReturnParameter);
            var hasNullableAnnotation = nullabilityInfo.WriteState == NullabilityState.Nullable ||
                                        nullabilityInfo.ReadState == NullabilityState.Nullable;

            if (resultType == typeof(void) || (res == null && hasNullableAnnotation))
                return new JobExectionResult();

            if (res == null)
                return Result.Error("Job hat unerwartet NULL zurückgegeben.");

            return new JobExectionResult { Value = res, Type = resultType };

        }
        catch (Exception ex) { return ex; }
    }

    #region private

    private static async Task<Result<object[]?>> CreateInvocationArguments(MethodInfo func, JAPClient japClient)
    {
        var parameters = func.GetParameters();
        if (parameters.Length == 0) return Result<object[]?>.Success(null);

        var rawArguments = await japClient.GetRawArguments();
        if (!rawArguments.Succeeded)
            return Result<object[]?>.Propagate(rawArguments.ToResult<object[]?>());

        var createInvicationArguments = RetrieveInvocationArguments(func, rawArguments.Value);
        if (!createInvicationArguments.Succeeded)
            return Result<object[]?>.Propagate(createInvicationArguments.ToResult<object[]?>());

        return createInvicationArguments.Value;
    }
    private static Result<Assembly> FindEntryPointAssembly(Type? entrypointType = null)
    {
        try
        {
            var asm = entrypointType != null ? Assembly.GetAssembly(entrypointType) : Assembly.GetEntryAssembly();
            if (asm != null) return asm;

            var sb = new StringBuilder();
            sb.AppendLine("Das Entrypoint-Assembly wurde nicht gefunden.");
            sb.AppendLine($"Explicit Enrypoint-Type: {(entrypointType == null ? "<NULL>" : entrypointType.FullName)}");
            return Result.Error(sb.ToString());
        }
        catch (Exception ex) { return ex; }
    }
    private static Result<MethodInfo> FindJobFunction(Assembly asm)
    {
        try
        {
            var methods = asm.GetExportedTypes()
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(method => method.GetCustomAttribute<JobEntryPointAttribute>() is not null).ToList();

            return methods.Count switch
            {
                0 => Result.Error($"Keine Methode mit dem Attibut [{nameof(JobEntryPointAttribute)}] in [{asm.FullName}] gefunden."),
                > 1 => Result.Error($"Mehrere Methoden mit dem Attibut [{nameof(JobEntryPointAttribute)}] in [{asm.FullName}] gefunden."),
                _ => methods.First()
            };
        }
        catch (Exception ex) { return ex; }
    }
    private static Result<object[]> RetrieveInvocationArguments(MethodInfo func, string rawArguments)
    {
        try
        {
            var parameters = func.GetParameters();
            if (parameters.Length == 0)
                return Array.Empty<object>();

            var isSingleRecord = (parameters.Length == 1) && parameters.Single().ParameterType.GetMethod("<Clone>$") is not null;

            if (!isSingleRecord)
            {
                var getInvokeArgs = PropertyBag.Deserialize(rawArguments, parameters);
                if (!getInvokeArgs.Succeeded)
                    return Result.Error(getInvokeArgs.ErrorMessage, getInvokeArgs.Exception);
                return getInvokeArgs.Value;
            }

            var getARgumentsRecord = PropertyBag.Deserialize(rawArguments, parameters.First().ParameterType);
            if (!getARgumentsRecord.Succeeded)
                return Result.Error(getARgumentsRecord.ErrorMessage, getARgumentsRecord.Exception);

            return new List<object> { getARgumentsRecord.Value }.ToArray();

        }
        catch (Exception ex) { return ex; }
    }

    #endregion
}
