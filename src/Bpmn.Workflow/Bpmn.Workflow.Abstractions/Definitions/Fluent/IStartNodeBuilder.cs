using Bpmn.Workflow.Documents;

namespace Bpmn.Workflow.Definitions.Fluent;

/// <summary>
/// Entry point of every graph. Exposes the node kinds that can begin a sequence.
/// </summary>
public interface IStartNodeBuilder<TDocument>
    where TDocument : class, IWorkflowDocument
{
    IServiceTaskBuilder<TDocument> ServiceTask(
        string id,
        Action<IServiceTaskOptions<TDocument>>? configure = null);

    IUserTaskBuilder<TDocument> UserTask(
        string id,
        Action<IUserTaskOptions<TDocument>>? configure = null);

    IExclusiveGatewayBuilder<TDocument> ExclusiveGateway(string id);

    IParallelGatewayBuilder<TDocument> ParallelGateway(string id);

    IInclusiveGatewayBuilder<TDocument> InclusiveGateway(string id);

    ITimerCatchEventBuilder<TDocument> TimerCatchEvent(
        string id,
        Action<ITimerCatchEventOptions> configure);

    IMessageCatchEventBuilder<TDocument> MessageCatchEvent(
        string id,
        Action<IMessageCatchEventOptions<TDocument>>? configure = null);

    IUpdateDocumentBuilder<TDocument> UpdateDocument(
        string id,
        Action<IUpdateDocumentOptions<TDocument>> configure);

    ISubProcessBuilder<TDocument> SubProcess(string id);
}
