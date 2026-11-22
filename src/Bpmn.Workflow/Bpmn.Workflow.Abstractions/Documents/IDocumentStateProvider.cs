namespace Bpmn.Workflow.Documents;

/// <summary>Untyped marker so the runtime can discover/register providers.</summary>
public interface IDocumentStateProvider { }

/// <summary>
/// Developer-implemented retrieval of the current authoritative state of a document
/// by its identity. This is the ONLY way the engine obtains a fresh document snapshot.
/// </summary>
/// <remarks>
/// Implementations may perform I/O (DB, HTTP, filesystem) and are expected to be
/// relatively expensive; the engine caches fetched snapshots across wait states
/// so that a resume does not re-invoke this method unless an UpdateDocument
/// activity is executed again.
/// </remarks>
public interface IDocumentStateProvider<TDocument> : IDocumentStateProvider
    where TDocument : class, IWorkflowDocument
{
    Task<TDocument> GetDocumentAsync(string documentId, CancellationToken cancellationToken);
}
