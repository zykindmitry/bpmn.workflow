using Bpmn.Workflow.Execution;

namespace Bpmn.Workflow.Extensibility;

/// <summary>
/// Optional evaluator for conditions expressed as data (for example, when integrating
/// with a rule engine). Delegate-based conditions produced by the fluent DSL do not
/// require this abstraction.
/// </summary>
public interface IExpressionEvaluator
{
    /// <summary>Evaluates an expression against the supplied execution context.</summary>
    bool Evaluate(string expression, IExecutionContext context);
}
