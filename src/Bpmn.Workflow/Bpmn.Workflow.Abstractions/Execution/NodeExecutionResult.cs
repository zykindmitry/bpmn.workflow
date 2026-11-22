namespace Bpmn.Workflow.Execution;

/// <summary>How a node finished.</summary>
public enum ExecutionStatus
{
    /// <summary>Continue immediately along <see cref="NodeExecutionResult.NextNodeIds"/>.</summary>
    Continue,

    /// <summary>The token is a wait state. The instance will be persisted and resumed later.</summary>
    Waiting,

    /// <summary>This branch or process reached an end.</summary>
    Completed,

    /// <summary>The node faulted. The instance is marked faulted.</summary>
    Faulted
}

/// <summary>
/// The engine's central contract. Every node expresses where to go next and whether it
/// is waiting by returning this shape.
/// </summary>
public sealed record NodeExecutionResult(
    IReadOnlyList<string> NextNodeIds,
    ExecutionStatus Status,
    IReadOnlyDictionary<string, object?>? VariableUpdates = null,
    string? WaitReason = null,
    string? CorrelationKey = null,
    string? FaultReason = null)
{
    /// <summary>Continues to one or more next nodes.</summary>
    public static NodeExecutionResult Continue(params string[] nextNodeIds) =>
        new(nextNodeIds ?? Array.Empty<string>(), ExecutionStatus.Continue);

    /// <summary>Marks the token as waiting for an external trigger.</summary>
    public static NodeExecutionResult Waiting(string? reason = null, string? correlationKey = null) =>
        new(Array.Empty<string>(), ExecutionStatus.Waiting, WaitReason: reason, CorrelationKey: correlationKey);

    /// <summary>Completes the token.</summary>
    public static NodeExecutionResult Completed() =>
        new(Array.Empty<string>(), ExecutionStatus.Completed);

    /// <summary>Faults the token with a reason.</summary>
    public static NodeExecutionResult Faulted(string reason) =>
        new(Array.Empty<string>(), ExecutionStatus.Faulted, FaultReason: reason);
}