using Bpmn.Workflow.Documents;

namespace Bpmn.Workflow.Execution;

/// <summary>
/// Untyped, engine-facing execution context. The engine works with this shape; typed
/// access to the document is available via <see cref="GetDocument{TDocument}"/>.
/// </summary>
public interface IExecutionContext
{
    /// <summary>Id of the workflow instance.</summary>
    string InstanceId { get; }

    /// <summary>Id of the token being executed.</summary>
    string TokenId { get; }

    /// <summary>Id of the node currently executing.</summary>
    string CurrentNodeId { get; }

    /// <summary>The current document snapshot as a runtime object.</summary>
    object Document { get; }

    /// <summary>
    /// Typed access to the current document. Throws <see cref="DocumentTypeMismatchException"/>
    /// if the instance's document type does not match <typeparamref name="TDocument"/>.
    /// </summary>
    TDocument GetDocument<TDocument>() where TDocument : class, IWorkflowDocument;

    /// <summary>Non-document process variables. Not the source of truth for workflow data.</summary>
    IReadOnlyDictionary<string, object?> Variables { get; }

    /// <summary>The scoped service provider for the current execution.</summary>
    IServiceProvider Services { get; }
}

/// <summary>
/// Typed context handed to conditions, assignee expressions, correlation key expressions,
/// and other developer-written expressions.
/// </summary>
public interface IExecutionContext<TDocument> : IExecutionContext
    where TDocument : class, IWorkflowDocument
{
    /// <summary>The current document snapshot.</summary>
    new TDocument Document { get; }
}
