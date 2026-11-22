using Bpmn.Workflow.FlowObjects;
using System.Threading;

namespace Bpmn.Workflow.Execution;

/// <summary>
/// Runtime behavior of a node. The framework supplies handlers for built-in nodes;
/// developers supply them for custom nodes. Resolved from DI.
/// </summary>
public interface IFlowNodeHandler
{
    /// <summary>The node kind this handler implements.</summary>
    FlowNodeType NodeType { get; }

    /// <summary>Executes the node and returns the engine's next-step instruction.</summary>
    Task<NodeExecutionResult> ExecuteAsync(
        IFlowNode node,
        IExecutionContext context,
        CancellationToken cancellationToken);
}
