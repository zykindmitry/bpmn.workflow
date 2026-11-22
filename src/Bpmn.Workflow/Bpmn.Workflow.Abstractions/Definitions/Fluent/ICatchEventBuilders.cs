using Bpmn.Workflow.Documents;
using Bpmn.Workflow.Execution;

namespace Bpmn.Workflow.Definitions.Fluent;

public interface ITimerCatchEventBuilder<TDocument>
    where TDocument : class, IWorkflowDocument
{
    IStartNodeBuilder<TDocument> Next();
}

public interface ITimerCatchEventOptions
{
    /// <summary>Absolute due time.</summary>
    ITimerCatchEventOptions DueAt(DateTimeOffset utc);

    /// <summary>Duration relative to entering the node.</summary>
    ITimerCatchEventOptions Duration(TimeSpan duration);
}

public interface IMessageCatchEventBuilder<TDocument>
    where TDocument : class, IWorkflowDocument
{
    IStartNodeBuilder<TDocument> Next();
}

public interface IMessageCatchEventOptions<TDocument>
    where TDocument : class, IWorkflowDocument
{
    /// <summary>
    /// Correlation key derived from the current document, used to route external messages
    /// to the correct waiting token.
    /// </summary>
    IMessageCatchEventOptions<TDocument> CorrelationKey(
        Func<IExecutionContext<TDocument>, string> keySelector);
}