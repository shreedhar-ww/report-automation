namespace DbComparisonApp.Attributes;

/// <summary>
/// Attribute to mark properties that should be excluded from comparison
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class CompareIgnoreAttribute : Attribute
{
}
