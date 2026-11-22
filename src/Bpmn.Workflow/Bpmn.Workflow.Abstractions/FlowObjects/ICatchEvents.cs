namespace Bpmn.Workflow.FlowObjects;

public interface ITimerCatchEvent : IFlowNode
{
    TimeSpan? Duration { get; }

    DateTimeOffset? DueAt { get; }
}

public interface IMessageCatchEvent : IFlowNode
{
    /// <summary>Id of the expression that computes the correlation key.</summary>
    string? CorrelationKeyExpressionId { get; }
}
