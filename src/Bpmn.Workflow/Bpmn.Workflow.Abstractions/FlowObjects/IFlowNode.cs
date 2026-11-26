namespace Bpmn.Workflow.FlowObjects;

/// <summary>Base contract for any node in the frozen process graph.</summary>
public interface IFlowNode
{
    /// <summary>Stable node identifier within the definition.</summary>
    string Id { get; }

    /// <summary>Human-readable name.</summary>
    string Name { get; }

    /// <summary>Kind of the node.</summary>
    FlowNodeType Type { get; }

    /// <summary>Ids of nodes with an incoming edge to this node.</summary>
    IReadOnlyList<string> Incoming { get; }

    /// <summary>Ids of nodes with an outgoing edge from this node.</summary>
    IReadOnlyList<string> Outgoing { get; }
}

public interface IStartEvent : IFlowNode { }

public interface IEndEvent : IFlowNode { }