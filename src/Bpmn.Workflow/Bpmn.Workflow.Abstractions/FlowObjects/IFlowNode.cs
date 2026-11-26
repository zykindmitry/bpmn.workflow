namespace Bpmn.Workflow.FlowObjects;

public interface IFlowNode
{
    string Id { get; }
    string? Name { get; }
    FlowNodeType Type { get; }
}

public interface IHasIncoming : IFlowNode
{
    IReadOnlyList<string> Incoming { get; }
}

public interface IHasOutgoing : IFlowNode
{
    IReadOnlyList<string> Outgoing { get; }
}

/// <summary>Entry point of a scope. Has outgoing edges only.</summary>
public interface IStartEvent : IHasOutgoing { }

/// <summary>Terminal node of a scope. Has incoming edges only.</summary>
public interface IEndEvent : IHasIncoming { }