//using JOSYN.Core.IPC;
//using JOSYN.Core.IPC.JIP;
//using JOSYN.Core.PropertyBag;
//using JOSYN.Core.ResultPattern;
//using System.Diagnostics;
//using System.Diagnostics.CodeAnalysis;
//using System.Text;

//namespace JOSYN.JobHost;

//internal class JobSessionIPCClient
//{
//    //private static ClientPipes? pipes = null!;

//    //[MemberNotNullWhen(true, nameof(pipes))]
//    //private static bool IsConnected { get; set; } = false;

//    internal static async Task<Result<ClientPipes>> ConnectAsync(string[] args)
//    {
//        var sessionKey = PipesProtocol.ParseSessionKeyCLIArguments(args);
//        if (sessionKey == Guid.Empty)
//            return Result.Error("Es wurde kein Pipes-SessionKey übergeben");
        
        
//        var getPipes = await PipesClient.ConnectAsync(sessionKey);
//        if (!getPipes.Succeeded)
//            return Result<ClientPipes>.Propagate(getPipes);            

//        //pipes = getPipes.Value;
//        //IsConnected = true;

//        return getPipes.Value;
//    }

//    internal static async Task<Result<string>> GetArguments(ClientPipes pipes)
//    {
//        //if (!IsConnected)
//        //    return Result<string>.Fail("Es wurde keine Verbindung hergestellt (ClientPipes sind NULL)");
        
//        var getConfig = await JipClient.SendAsync<string>(pipes, "GET-CONFIG", d => d);
        
//        if (!getConfig.Succeeded) return 
//            Result<string>.Propagate(getConfig);


//        //var res = await PipesClient.SendAsync("GET-ARGUMENTS"u8.ToArray(), pipes);
//        //if (!res.Succeeded)
//        //    return Result<string>.Propagate(res.ToResult<string>());

//        return getConfig.Value;

//        //return await Task.FromResult( Result<string>.Fail("aaaaaaaaaaaaaaaaa") );
//    }

//    internal static void ProcessResult(JobExectionResult result)
//    {
//        //Console.ForegroundColor = ConsoleColor.Green;
//        //Console.WriteLine("Der Job wurde erfolgreich ausgeführt.\n");

//        //if (result is { Value: not null, Type: not null })
//        //{
//        //    // TODO
//        //    // Achtung, hier später kein Default-Format verdrahten! => IniDictionarySerializer.Serialize
//        //    var getResultAsString = PropertyBag.Serialize(result.Value, result.Type, IniDictionarySerializer.Serialize);
//        //    if (!getResultAsString.Succeeded)
//        //    {
//        //        ProcessError(getResultAsString.ErrorMessage, getResultAsString.CallStackToString(), getResultAsString.Exception);
//        //        return;
//        //    }

//        //    Console.WriteLine("[JobResult]");
//        //    Console.WriteLine(getResultAsString.Value);
//        //}

//        //Console.ResetColor();
//        //Console.WriteLine("\n[Press Any Key]\n");
//        //Console.ReadKey();
//    }



//    //internal static async Task Test()
//    //{
//    //    var request = new RequestMessage
//    //    {
//    //        FunctionName = "MyFunctionName",
//    //        Arguments = new Dictionary<string, string> { { "arg1", "value1" }, { "arg2", "value2" } },
//    //        Payload = "This is the payload data.",

//    //    };
//    //    var requestStr = System.Text.Json.JsonSerializer.Serialize(request);

//    //    var res = await PipesClient.SendAsync(Encoding.UTF8.GetBytes(requestStr), pipes!);

//    //    if (!res.Succeeded)
//    //    {
//    //        Console.ForegroundColor = ConsoleColor.Red;
//    //        Console.WriteLine(res.ErrorMessage);
//    //        Console.WriteLine(res.Exception);
//    //        Console.ResetColor();
//    //        return;
//    //    }

//    //    Console.WriteLine(Encoding.UTF8.GetString(res.Value));
//    //    Console.ForegroundColor = ConsoleColor.Green;
//    //    Console.WriteLine("[Antwort vom IPC Server]");
//    //    Console.WriteLine(Encoding.UTF8.GetString(res.Value));
//    //    Console.WriteLine();
//    //    Console.ResetColor();
//    //}






//    #region FakeLog

//    private static int LogErrorResult(Result result, int exitCode)
//    {
//        var sb = new StringBuilder();
//        sb.AppendLine("[Errormessage]");
//        sb.AppendLine(result.ErrorMessage);
//        sb.AppendLine();
//        sb.AppendLine("[Callstack]");
//        sb.AppendLine(result.CallStackAsString);
//        if (result.Exception != null)
//        {
//            sb.AppendLine();
//            sb.AppendLine("[Exception]");
//            sb.AppendLine(result.Exception.ToString());
//        }
//        return LogError(sb.ToString(), exitCode);
//    }

//    private static int LogError(string msg, int exitCode)
//    {
//        Console.ForegroundColor = ConsoleColor.Red;
//        Console.WriteLine(msg);
//        Console.ResetColor();
//        Console.WriteLine("\n[PRESS ANY KEY]");
//        Console.ReadKey();
//        return exitCode;
//    }



//    #endregion









//}