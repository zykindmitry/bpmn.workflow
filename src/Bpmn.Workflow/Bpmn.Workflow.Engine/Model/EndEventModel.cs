using Bpmn.Workflow.FlowObjects;

namespace Bpmn.Workflow.Model;

internal sealed class EndEventModel : FlowNodeModel, IEndEvent
{
    internal EndEventModel(string id, string? name, IEnumerable<string> incoming)
        : base(id, name, FlowNodeType.EndEvent, incoming, outgoing: null) { }
}
