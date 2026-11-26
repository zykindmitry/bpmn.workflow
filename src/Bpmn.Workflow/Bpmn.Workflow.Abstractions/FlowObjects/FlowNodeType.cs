namespace Bpmn.Workflow.FlowObjects;

/// <summary>Kinds of nodes the engine recognizes in the frozen graph.</summary>
public enum FlowNodeType
{
    StartEvent,
    EndEvent,
    ServiceTask,
    UserTask,
    UpdateDocument,
    ExclusiveGateway,
    ParallelGateway,
    InclusiveGateway,
    TimerCatchEvent,
    MessageCatchEvent,
    SubProcess
}
