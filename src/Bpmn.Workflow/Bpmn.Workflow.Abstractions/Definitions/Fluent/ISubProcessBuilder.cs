using Bpmn.Workflow.Documents;

namespace Bpmn.Workflow.Definitions.Fluent;

public interface ISubProcessBuilder<TDocument>
    where TDocument : class, IWorkflowDocument
{
    /// <summary>Defines the subprocess body (scoped to the subprocess).</summary>
    IStartNodeBuilder<TDocument> Body();

    /// <summary>Continues after the subprocess.</summary>
    IStartNodeBuilder<TDocument> Next();
}
