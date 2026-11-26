namespace Bpmn.Workflow.FlowObjects;

public interface ITimerCatchEvent : IHasIncoming, IHasOutgoing
{
    TimeSpan? Duration { get; }

    DateTimeOffset? DueAt { get; }
}

public interface IMessageCatchEvent : IHasIncoming, IHasOutgoing
{
    /// <summary>Id of the expression that computes the correlation key.</summary>
    string? CorrelationKeyExpressionId { get; }
}
