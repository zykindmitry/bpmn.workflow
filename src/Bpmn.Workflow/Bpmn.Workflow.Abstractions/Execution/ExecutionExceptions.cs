namespace Bpmn.Workflow.Execution;

/// <summary>Base for failures raised by the execution pipeline.</summary>
public class WorkflowExecutionException : Exception
{
    public WorkflowExecutionException(string message) : base(message) { }

    public WorkflowExecutionException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Raised by <see cref="IExecutionContext.GetDocument{TDocument}"/> when the runtime
/// document type does not match the caller's requested type.
/// </summary>
public class DocumentTypeMismatchException : WorkflowExecutionException
{
    public DocumentTypeMismatchException(Type expected, Type actual)
        : base($"Instance document type '{actual.FullName}' does not match definition's '{expected.FullName}'.")
    {
        Expected = expected;
        Actual = actual;
    }

    public Type Expected { get; }

    public Type Actual { get; }
}
