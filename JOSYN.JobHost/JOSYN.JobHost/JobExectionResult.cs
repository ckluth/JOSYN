namespace JOSYN.JobHost;

public record JobExectionResult
{
    public object? Value;
    public Type? Type;
}