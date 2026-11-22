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
    /// <summary>
    /// Computes the assignee set as an array of user principals. Must return at least
    /// one principal; an empty array at entry faults the token with
    /// "user-task-no-assignees". Groups/roles are resolved to concrete users here.
    /// </summary>
    IUserTaskOptions<TDocument> Assignees(
        Func<IExecutionContext<TDocument>, IReadOnlyCollection<string>> assignees);

    /// <summary>Convenience overload for a single principal.</summary>
    IUserTaskOptions<TDocument> Assignee(
        Func<IExecutionContext<TDocument>, string> assignee);
}
