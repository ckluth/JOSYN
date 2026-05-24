namespace JOSYN.System.Frontend.Attributes;

/// <summary>
/// Kennzeichnet eine Klasse als Ergebnistyp eines Jobs.
/// Wird verwendet, um den Rückgabetyp der <see cref="JobEntryPointAttribute"/>-Methode
/// explizit als Job-Ergebnis zu markieren.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class JobResultAttribute() : Attribute { }