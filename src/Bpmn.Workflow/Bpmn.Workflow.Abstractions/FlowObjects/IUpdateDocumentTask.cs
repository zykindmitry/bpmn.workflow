namespace Bpmn.Workflow.FlowObjects;

/// <summary>
/// A node that instructs the engine to re-fetch the document from the developer's
/// <see cref="Documents.IDocumentStateProvider{TDocument}"/> and replace the in-flight
/// snapshot with the fetched state.
/// </summary>
public interface IUpdateDocumentTask : IFlowNode
{
    /// <summary>Descriptor for the developer's <c>IDocumentStateProvider&lt;TDocument&gt;</c>.</summary>
    HandlerDescriptor Provider { get; }

    /// <summary>Where the token continues after a successful refresh.</summary>
    string NextNodeId { get; }

    /// <summary>
    /// Optional target for <see cref="Documents.DocumentNotFoundException"/>.
    /// If null, a not-found outcome faults the instance.
    /// </summary>
    string? NotFoundNodeId { get; }
}
