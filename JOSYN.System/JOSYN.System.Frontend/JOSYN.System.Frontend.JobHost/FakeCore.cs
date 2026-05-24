using JOSYN.Foundation.ResultPattern;

namespace JOSYN.System.Frontend.JobHost;

internal static class FakeCore
{
    internal  static void LogLocalError(Result result)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Keine IPC-Server verfügbar. Fehler wird lokal gespeichert.\n");
        Console.WriteLine(result.ErrorMessage);
        Console.WriteLine(result.Exception);
        Console.ResetColor();
        //Console.ReadKey();
    }

    internal static void LogLocalError(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Keine IPC-Server verfügbar. Fehler wird lokal gespeichert.\n");
        Console.WriteLine(msg);
        Console.ResetColor();
        //Console.ReadKey();
    }

    internal static void LogError(Result result)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Fehler wird an IPC-Server geschickt (und lokal gespeichert).\n");
        Console.WriteLine(result.ErrorMessage);
        Console.WriteLine(result.Exception);
        Console.ResetColor();
        //Console.ReadKey();
    }

    internal static void LogError(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Fehler wird an IPC-Server geschickt (und lokal gespeichert).\n");
        Console.WriteLine(msg);
        Console.ResetColor();
        //Console.ReadKey();
    }

}


//#region FakeLog

//private static int LogErrorResult(Result result, int exitCode)
//{
//    var sb = new StringBuilder();
//    sb.AppendLine("[Errormessage]");
//    sb.AppendLine(result.ErrorMessage);
//    sb.AppendLine();
//    sb.AppendLine("[Callstack]");
//    sb.AppendLine(result.CallStackAsString);
//    if (result.Exception != null)
//    {
//        sb.AppendLine();
//        sb.AppendLine("[Exception]");
//        sb.AppendLine(result.Exception.ToString());
//    }
//    return LogError(sb.ToString(), exitCode);
//}

//private static int LogError(string msg, int exitCode)
//{
//    Console.ForegroundColor = ConsoleColor.Red;
//    Console.WriteLine(msg);
//    Console.ResetColor();
//    Console.WriteLine("\n[PRESS ANY KEY]");
//    Console.ReadKey();
//    return exitCode;
//}



//#endregion
