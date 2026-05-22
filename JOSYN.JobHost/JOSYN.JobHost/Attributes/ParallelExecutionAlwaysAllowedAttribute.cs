namespace JOSYN.JobHost.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class ParallelExecutionAllowedAttribute(bool isAllowed = true) : Attribute
{
    private void X()
    {
        if (isAllowed )
        {
            // foo
        }
    } 
    
}