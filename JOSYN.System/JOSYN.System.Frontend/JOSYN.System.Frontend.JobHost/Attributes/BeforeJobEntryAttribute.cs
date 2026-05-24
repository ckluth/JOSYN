namespace JOSYN.System.Frontend.JobHost.Attributes;

/// <summary>
/// Kennzeichnet eine Methode, die vor dem Einstiegspunkt eines Jobs ausgeführt wird.
/// Eignet sich für Initialisierungsarbeiten wie das Einrichten von Ressourcen oder
/// das Auswerten von Bedingungen (z. B. ob parallele Ausführung erlaubt ist),
/// bevor der eigentliche Job gestartet wird.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class BeforeJobEntryPointAttribute() : Attribute { }