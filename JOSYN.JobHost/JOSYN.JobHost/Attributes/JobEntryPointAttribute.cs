namespace JOSYN.JobHost.Attributes;

/// <summary>
/// Dieses Attribute kennzeichnet die Methode, die als Einstiegspunkt für einen Job dient. Es muss genau eine Methode pro Job mit diesem Attribut geben.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class JobEntryPointAttribute() : Attribute { }