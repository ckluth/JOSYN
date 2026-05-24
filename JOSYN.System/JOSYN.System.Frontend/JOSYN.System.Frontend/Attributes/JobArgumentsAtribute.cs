namespace JOSYN.System.Frontend.Attributes;

/// <summary>
/// Kennzeichnet eine Klasse als Argumenttyp für einen Job.
/// Wird verwendet, um den Parameter-Typ der <see cref="JobEntryPointAttribute"/>-Methode
/// explizit als Job-Argumente zu markieren.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class JobArgumentsAttribute() : Attribute { }