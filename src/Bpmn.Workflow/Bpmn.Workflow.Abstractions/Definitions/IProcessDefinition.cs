using Bpmn.Workflow.FlowObjects;

namespace Bpmn.Workflow.Definitions;

/// <summary>
/// Untyped, immutable process definition produced by the fluent builder.
/// The engine consumes this shape and never needs the concrete document type.
/// </summary>
public interface IProcessDefinition
{
    /// <summary>Stable identifier of the definition, unique across versions.</summary>
    string Id { get; }

    /// <summary>Monotonic version of the definition. Instances pin to a single version.</summary>
    int Version { get; }

    /// <summary>Human-readable name, used for diagnostics and UI.</summary>
    string Name { get; }

    /// <summary>The single start node of the process.</summary>
    IFlowNode Start { get; }

    /// <summary>All nodes in the graph, keyed by node id.</summary>
    IReadOnlyDictionary<string, IFlowNode> Nodes { get; }

    /// <summary>
    /// Deterministic structural hash over node ids, node types, edges, handler descriptors,
    /// and developer-supplied condition ids. Used to pin instances to the exact structural
    /// version they were started under.
    /// </summary>
    string DefinitionHash { get; }

    /// <summary>Concrete CLR type of the document this definition operates on.</summary>
    Type DocumentType { get; }
}

/// <summary>Typed view of an <see cref="IProcessDefinition"/>.</summary>
public interface IProcessDefinition<TDocument> : IProcessDefinition
    where TDocument : class, Documents.IWorkflowDocument
{
}
