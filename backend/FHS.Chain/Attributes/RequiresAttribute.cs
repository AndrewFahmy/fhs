namespace FHS.Chain.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class RequiresAttribute(params string[] fields) : Attribute
{
    public IReadOnlyList<string> Fields { get; } = fields;
}
