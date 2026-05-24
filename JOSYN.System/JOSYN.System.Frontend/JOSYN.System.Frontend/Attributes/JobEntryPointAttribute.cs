namespace JOSYN.System.Frontend.Attributes;

/// <summary>
/// Kennzeichnet die Methode, die als Einstiegspunkt für einen Job dient.
/// Pro Job-Assembly darf genau eine Methode dieses Attribut tragen.
/// Die Methode muss <c>public static</c> sein.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public sealed class JobEntryPointAttribute() : Attribute { }