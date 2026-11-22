using Bpmn.Workflow.Definitions;

namespace Bpmn.Workflow.Extensibility;

/// <summary>
/// Resolves process definitions by id and version. The default implementation is an
/// in-assembly registry populated at startup; alternative implementations may load from
/// other sources.
/// </summary>
public interface IWorkflowDefinitionProvider
{
    /// <summary>All registered definitions, across all versions.</summary>
    IReadOnlyCollection<IProcessDefinition> All { get; }

    /// <summary>Gets the definition with the exact id and version. Throws if not found.</summary>
    IProcessDefinition Get(string id, int version);

    /// <summary>Tries to get a definition without throwing.</summary>
    bool TryGet(string id, int version, out IProcessDefinition? definition);
}
