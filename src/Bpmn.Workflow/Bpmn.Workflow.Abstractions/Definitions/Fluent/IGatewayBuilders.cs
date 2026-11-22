using Bpmn.Workflow.Documents;
using Bpmn.Workflow.Execution;

namespace Bpmn.Workflow.Definitions.Fluent;

public interface IExclusiveGatewayBuilder<TDocument>
    where TDocument : class, IWorkflowDocument
{
    /// <summary>Adds a branch with a stable id and a condition over the typed document.</summary>
    IBranchBuilder<TDocument> Branch(
        string branchId,
        Action<IBranchOptions<TDocument>> configure);
}

public interface IInclusiveGatewayBuilder<TDocument>
    where TDocument : class, IWorkflowDocument
{
    /// <summary>Adds a branch with a stable id and a condition over the typed document.</summary>
    IBranchBuilder<TDocument> Branch(
        string branchId,
        Action<IBranchOptions<TDocument>> configure);
}

public interface IParallelGatewayBuilder<TDocument>
    where TDocument : class, IWorkflowDocument
{
    /// <summary>Adds an unconditional branch.</summary>
    IBranchBuilder<TDocument> Branch(string branchId);
}

public interface IBranchBuilder<TDocument>
    where TDocument : class, IWorkflowDocument
{
    /// <summary>Starts the branch body.</summary>
    IStartNodeBuilder<TDocument> Then();
}

public interface IBranchOptions<TDocument>
    where TDocument : class, IWorkflowDocument
{
    /// <summary>
    /// Sets the branch condition. The <paramref name="conditionId"/> is a developer-supplied
    /// stable identifier and is an input to the definition hash; refactoring the lambda body
    /// does not change the hash, but changing the id does.
    /// </summary>
    IBranchOptions<TDocument> Condition(
        string conditionId,
        Func<IExecutionContext<TDocument>, bool> predicate);
}
