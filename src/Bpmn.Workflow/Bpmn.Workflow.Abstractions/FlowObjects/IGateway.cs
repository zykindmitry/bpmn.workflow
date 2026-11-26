namespace Bpmn.Workflow.FlowObjects;

public interface IGateway : IHasIncoming, IHasOutgoing
{
    IReadOnlyList<IBranchDefinition> Branches { get; }
}

public interface IBranchDefinition
{
    /// <summary>Stable branch identifier within the gateway.</summary>
    string BranchId { get; }

    /// <summary>Id of the node the branch continues to.</summary>
    string TargetNodeId { get; }

    /// <summary>
    /// Developer-supplied condition id. Participates in the definition hash.
    /// Null for parallel gateways.
    /// </summary>
    string? ConditionId { get; }
}