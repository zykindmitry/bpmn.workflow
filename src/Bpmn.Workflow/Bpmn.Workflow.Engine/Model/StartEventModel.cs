using Bpmn.Workflow.FlowObjects;

namespace Bpmn.Workflow.Model;

internal sealed class StartEventModel : FlowNodeModel, IStartEvent
{
    internal StartEventModel(string id, string? name, IEnumerable<string> outgoing)
        : base(id, name, FlowNodeType.StartEvent, incoming: null, outgoing: outgoing) { }
}
