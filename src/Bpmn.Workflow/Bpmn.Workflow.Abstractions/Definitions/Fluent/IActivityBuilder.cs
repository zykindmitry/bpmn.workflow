using Bpmn.Workflow.Documents;

namespace Bpmn.Workflow.Definitions.Fluent;

/// <summary>Common chaining surface for activity nodes that continue in sequence.</summary>
public interface IActivityBuilder<TDocument>
    where TDocument : class, IWorkflowDocument
{
    /// <summary>Continues with the next node in sequence.</summary>
    IStartNodeBuilder<TDocument> Next();
}
