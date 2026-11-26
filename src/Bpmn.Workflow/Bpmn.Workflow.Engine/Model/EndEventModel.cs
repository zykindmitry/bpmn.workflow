using System.Collections.Immutable;
using Bpmn.Workflow.FlowObjects;

namespace Bpmn.Workflow.Model;

/// <summary>
/// The single concrete implementation of an end event.
/// Has incoming edges only; there is no outgoing constructor parameter.
/// Cannot be subclassed.
/// </summary>
public sealed class EndEventModel : FlowNodeModel, IEndEvent
{
    public IReadOnlyList<string> Incoming { get; }

    public EndEventModel(string id, string? name, IEnumerable<string> incoming)
        : base(id, name, FlowNodeType.EndEvent)
    {
        ArgumentNullException.ThrowIfNull(incoming);
        Incoming = incoming.ToImmutableArray();
    }
}
