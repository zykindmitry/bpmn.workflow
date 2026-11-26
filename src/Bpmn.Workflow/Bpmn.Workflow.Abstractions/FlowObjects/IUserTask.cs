namespace Bpmn.Workflow.FlowObjects;

public interface IUserTask : IHasIncoming, IHasOutgoing
{
    /// <summary>
    /// Id of the expression that computes the assignee set from the current document.
    /// The expression returns one or more user principals; the engine snapshots the set
    /// when the token enters the task. Groups are not modeled — the expression is
    /// responsible for resolving any group/role membership to concrete users.
    /// </summary>
    string AssigneesExpressionId { get; }
}