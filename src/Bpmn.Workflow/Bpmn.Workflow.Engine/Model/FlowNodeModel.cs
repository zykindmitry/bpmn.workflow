using Bpmn.Workflow.FlowObjects;

namespace Bpmn.Workflow.Model;

/// <summary>
/// Immutable base for flow nodes in the frozen process graph.
/// </summary>
/// <remarks>
/// Edge collections are not part of this base class. Nodes that have incoming or outgoing
/// edges derive from <see cref="FlowNodeModel"/> and additionally implement
/// <see cref="IHasIncoming"/> / <see cref="IHasOutgoing"/> as appropriate.
/// Start and end events are <c>sealed</c> and constructed only by the engine.
/// </remarks>
public abstract class FlowNodeModel : IFlowNode
{
    public string Id { get; }
    public string? Name { get; }
    public FlowNodeType Type { get; }

    protected FlowNodeModel(string id, string? name, FlowNodeType type)
    {
        Id = string.IsNullOrWhiteSpace(id)
                 ? throw new ArgumentException("Node id must not be empty.", nameof(id))
                 : id;
        Name = name;
        Type = type;
    }

    public override string ToString() => $"{Type}:{Id}";
}
