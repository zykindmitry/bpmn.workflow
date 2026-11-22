using Bpmn.Workflow.Documents;
using Bpmn.Workflow.Execution;

namespace Bpmn.Workflow.Definitions.Fluent;

public interface IUserTaskBuilder<TDocument> : IActivityBuilder<TDocument>
    where TDocument : class, IWorkflowDocument
{
}

public interface IUserTaskOptions<TDocument>
    where TDocument : class, IWorkflowDocument
{
    /// <summary>Expression that computes the assignee from the current document state.</summary>
    IUserTaskOptions<TDocument> Assignee(Func<IExecutionContext<TDocument>, string> assignee);

    /// <summary>Static candidate group.</summary>
    IUserTaskOptions<TDocument> CandidateGroup(string group);

    /// <summary>UI form key associated with the task.</summary>
    IUserTaskOptions<TDocument> FormKey(string key);
}
