using System.Collections.Immutable;
using Bpmn.Workflow.FlowObjects;

namespace Bpmn.Workflow.Model;

/// <summary>
/// Immutable base class for flow nodes in the frozen process graph.
/// </summary>
/// <remarks>
/// <para>
/// Consumers may derive from this class to define custom node types, or implement
/// <see cref="IFlowNode"/> directly if they prefer not to depend on the engine package.
/// </para>
/// <para>
/// All state is captured in the constructor; subclasses must not expose mutable state.
/// Instances are canonical — two nodes in the same <c>ProcessDefinition</c> are compared
/// by reference, not by value.
/// </para>
/// </remarks>
public abstract class FlowNodeModel : IFlowNode
{
    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public string? Name { get; }

    /// <inheritdoc />
    public FlowNodeType Type { get; }

    /// <inheritdoc />
    public IReadOnlyList<string> Incoming { get; }

    /// <inheritdoc />
    public IReadOnlyList<string> Outgoing { get; }

    /// <summary>
    /// Initializes a new flow-node model.
    /// </summary>
    /// <param name="id">Stable, unique node identifier within the definition.</param>
    /// <param name="name">Optional human-readable node name.</param>
    /// <param name="type">Node kind, as recognized by the engine and its validators.</param>
    /// <param name="incoming">Ids of incoming edges; <see langword="null"/> means none.</param>
    /// <param name="outgoing">Ids of outgoing edges; <see langword="null"/> means none.</param>
    /// <exception cref="ArgumentException"><paramref name="id"/> is empty or whitespace.</exception>
    protected FlowNodeModel(
        string id,
        string? name,
        FlowNodeType type,
        IEnumerable<string>? incoming = null,
        IEnumerable<string>? outgoing = null)
    {
        Id = string.IsNullOrWhiteSpace(id)
                 ? throw new ArgumentException("Node id must not be empty.", nameof(id))
                 : id;
        Name = name;
        Type = type;
        Incoming = incoming is null ? ImmutableArray<string>.Empty : incoming.ToImmutableArray();
        Outgoing = outgoing is null ? ImmutableArray<string>.Empty : outgoing.ToImmutableArray();
    }

    /// <inheritdoc />
    public override string ToString() => $"{Type}:{Id}";
}
