using Bpmn.Workflow.Documents;

namespace Bpmn.Workflow.Definitions.Fluent;

/// <summary>
/// Configures an activity that re-fetches the document from its authoritative source
/// and replaces the in-flight snapshot. All subsequent activities see the fetched state.
/// </summary>
public interface IUpdateDocumentBuilder<TDocument>
    where TDocument : class, IWorkflowDocument
{
    /// <summary>Routes the token after a successful refresh. Required.</summary>
    IUpdateDocumentBuilder<TDocument> OnRefreshed(Action<IStartNodeBuilder<TDocument>> configure);

    /// <summary>
    /// Optional branch for <see cref="DocumentNotFoundException"/>.
    /// If not configured, a not-found outcome faults the instance.
    /// </summary>
    IUpdateDocumentBuilder<TDocument> OnNotFound(Action<IStartNodeBuilder<TDocument>> configure);
}

public interface IUpdateDocumentOptions<TDocument>
    where TDocument : class, IWorkflowDocument
{
    /// <summary>References the developer-implemented state provider by CLR type.</summary>
    IUpdateDocumentOptions<TDocument> Provider<TProvider>()
        where TProvider : class, IDocumentStateProvider<TDocument>;

    /// <summary>References the state provider by key. Used for plugin-style deployments.</summary>
    IUpdateDocumentOptions<TDocument> ProviderKey(string key);
}