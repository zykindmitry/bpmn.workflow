namespace Bpmn.Workflow.FlowObjects;

/// <summary>
/// Identifies a runtime-resolved component without instantiating it. The engine resolves
/// descriptors from DI (by type) or from a keyed registry (by key).
/// </summary>
public sealed record HandlerDescriptor(Type? HandlerType, string? HandlerKey)
{
    /// <summary>Creates a descriptor resolved by CLR type.</summary>
    public static HandlerDescriptor ByType(Type type) =>
        new(type ?? throw new ArgumentNullException(nameof(type)), null);

    /// <summary>Creates a descriptor resolved by key.</summary>
    public static HandlerDescriptor ByKey(string key) =>
        new(null, string.IsNullOrWhiteSpace(key) ? throw new ArgumentException("Key required.", nameof(key)) : key);
}