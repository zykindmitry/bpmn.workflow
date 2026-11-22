namespace Bpmn.Workflow.FlowObjects;

/// <summary>An executable node whose behavior is supplied by a handler.</summary>
public interface IActivity : IFlowNode
{
    HandlerDescriptor Handler { get; }
}
