namespace JOSYN.JobHost;

public interface ICore
{
    /// <summary>
    /// Run ist der EntryPoint jeder Job-Exe.
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static abstract Task<int> Run(string[] args);
}