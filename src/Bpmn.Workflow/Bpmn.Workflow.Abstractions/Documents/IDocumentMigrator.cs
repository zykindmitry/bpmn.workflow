namespace Bpmn.Workflow.Documents;

/// <summary>
/// Optional schema-drift hook. Invoked by the engine on instance load when the persisted
/// document's schema version is older than the current type's expected version.
/// </summary>
/// <remarks>
/// This concerns the persisted snapshot, not the authoritative source. When a snapshot is
/// upgraded here, the engine treats the result as the new in-memory document and persists
/// it on the next commit.
/// </remarks>
public interface IDocumentMigrator<TDocument>
    where TDocument : class, IWorkflowDocument
{
    /// <summary>Schema version the migrator accepts.</summary>
    int FromVersion { get; }

    /// <summary>Schema version the migrator produces.</summary>
    int ToVersion { get; }

    /// <summary>Transforms a document from <see cref="FromVersion"/> to <see cref="ToVersion"/>.</summary>
    TDocument Migrate(TDocument document);
}
