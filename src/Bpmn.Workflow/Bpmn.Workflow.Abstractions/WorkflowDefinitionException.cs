namespace Bpmn.Workflow;

/// <summary>
/// Raised when a process definition is structurally invalid.
/// Thrown by the fluent builder's <c>Build()</c> and by the runtime startup validator.
/// </summary>
public class WorkflowDefinitionException : Exception
{
    public WorkflowDefinitionException(string message) : base(message) { }
    public WorkflowDefinitionException(string message, Exception inner) : base(message, inner) { }
}
