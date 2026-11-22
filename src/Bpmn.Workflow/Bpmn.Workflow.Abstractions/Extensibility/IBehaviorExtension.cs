using Bpmn.Workflow.Execution;

namespace Bpmn.Workflow.Extensibility;

/// <summary>
/// Cross-cutting hooks around node execution (logging, auditing, SLA timers, metrics).
/// Registered extensions run in registration order.
/// </summary>
public interface IBehaviorExtension
{
    /// <summary>Called before a node handler executes.</summary>
    Task OnEnterAsync(IExecutionContext context, CancellationToken cancellationToken);

    /// <summary>Called after a node handler returns, before the engine applies the result.</summary>
    Task OnExitAsync(
        IExecutionContext context,
        NodeExecutionResult result,
        CancellationToken cancellationToken);
}
