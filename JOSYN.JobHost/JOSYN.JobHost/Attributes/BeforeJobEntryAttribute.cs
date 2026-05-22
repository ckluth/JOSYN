namespace JOSYN.JobHost.Attributes;

/// <summary>
/// Dieses Attribute kennzeichnet eine Methode, die vor dem Einstiegspunkt eines Jobs ausgeführt wird. 
/// Diese Methode kann verwendet werden, um Vorbereitungsarbeiten durchzuführen, wie z.B. das Einrichten von Ressourcen
/// oder das Überprüfen von Bedingungen (wie ConditionalParallelExecutionAllowed), bevor der eigentliche Job gestartet wird. 
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class BeforeJobEntryPointAttribute() : Attribute { }