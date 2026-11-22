using Bpmn.Workflow.Documents;
using Bpmn.Workflow.Execution;

namespace Bpmn.Workflow.Definitions.Fluent;

public interface IServiceTaskBuilder<TDocument> : IActivityBuilder<TDocument>
    where TDocument : class, IWorkflowDocument
{
}

public interface IServiceTaskOptions<TDocument>
    where TDocument : class, IWorkflowDocument
{
    /// <summary>References a handler by CLR type. Resolved from DI at runtime.</summary>
    IServiceTaskOptions<TDocument> Handler<THandler>()
        where THandler : class, IFlowNodeHandler;

    /// <summary>References a handler by key. Used for plugin-style deployments.</summary>
    IServiceTaskOptions<TDocument> HandlerKey(string key);
}
