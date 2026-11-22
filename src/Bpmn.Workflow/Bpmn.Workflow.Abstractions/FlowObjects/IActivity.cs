namespace Bpmn.Workflow.FlowObjects;

/// <summary>An executable node whose behavior is supplied by a handler.</summary>
public interface IActivity : IFlowNode
{
    HandlerDescriptor Handler { get; }
}

public interface IServiceTask : IActivity { }

public interface IUserTask : IFlowNode
{
    /// <summary>Id of the expression that computes the assignee, if any.</summary>
    string? AssigneeExpressionId { get; }

    string? CandidateGroup { get; }

    string? FormKey { get; }
}