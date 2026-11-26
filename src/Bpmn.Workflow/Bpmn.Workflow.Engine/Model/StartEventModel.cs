using System.Collections.Immutable;
using Bpmn.Workflow.FlowObjects;

namespace Bpmn.Workflow.Model;

/// <summary>
/// The single concrete implementation of a start event.
/// Has outgoing edges only; there is no incoming constructor parameter.
/// Cannot be subclassed — custom "start" semantics must be modelled as a node
/// that follows the start event.
/// </summary>
public sealed class StartEventModel : FlowNodeModel, IStartEvent
{
    public IReadOnlyList<string> Outgoing { get; }

    public StartEventModel(string id, string? name, IEnumerable<string> outgoing)
        : base(id, name, FlowNodeType.StartEvent)
    {
        ArgumentNullException.ThrowIfNull(outgoing);
        Outgoing = outgoing.ToImmutableArray();
    }
}
