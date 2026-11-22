namespace Bpmn.Workflow.Documents;

/// <summary>
/// Optional: distinguishes "not found" from other failures so the engine can
/// fault deterministically vs. retry transiently.
/// </summary>
public sealed class DocumentNotFoundException : Exception
{
    public DocumentNotFoundException(string documentId)
        : base($"Document '{documentId}' was not found.") => DocumentId = documentId;

    public string DocumentId { get; }
}
