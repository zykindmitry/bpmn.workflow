using System.Collections.Immutable;
using Bpmn.Workflow.FlowObjects;

namespace Bpmn.Workflow.Model;

/// <summary>
/// Base for flow nodes that participate in edges on both sides
/// (service tasks, user tasks, gateways, catch events, update-document tasks, sub-processes).
/// </summary>
public abstract class EdgedFlowNodeModel : FlowNodeModel, IHasIncoming, IHasOutgoing
{
    public IReadOnlyList<string> Incoming { get; }
    public IReadOnlyList<string> Outgoing { get; }

    protected EdgedFlowNodeModel(
        string id,
        string? name,
        FlowNodeType type,
        IEnumerable<string>? incoming = null,
        IEnumerable<string>? outgoing = null)
        : base(id, name, type)
    {
        Incoming = incoming is null ? ImmutableArray<string>.Empty : incoming.ToImmutableArray();
        Outgoing = outgoing is null ? ImmutableArray<string>.Empty : outgoing.ToImmutableArray();
    }
}