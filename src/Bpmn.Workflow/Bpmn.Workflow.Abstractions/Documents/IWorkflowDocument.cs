namespace Bpmn.Workflow.Documents;

/// <summary>
/// Marker/base contract for a developer-defined workflow document.
/// The document is a snapshot; the authoritative state is owned by the developer's
/// <see cref="IDocumentStateProvider{TDocument}"/> and re-read at UpdateDocument activities.
/// </summary>
public interface IWorkflowDocument
{
    /// <summary>Stable identity of the document (used as the retrieval key).</summary>
    string DocumentId { get; }
}