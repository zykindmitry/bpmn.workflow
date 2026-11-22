using Bpmn.Workflow.Documents;

namespace Bpmn.Workflow.Definitions.Fluent;

/// <summary>
/// Untyped builder surface. Used by infrastructure (assembly scanning) only.
/// Developers author against <see cref="IWorkflowBuilder{TDocument}"/>.
/// </summary>
public interface IWorkflowBuilder
{
    /// <summary>Produces an immutable process definition from the accumulated graph.</summary>
    IProcessDefinition Build();
}

/// <summary>Typed fluent DSL root. Each setter returns the same builder for chaining.</summary>
public interface IWorkflowBuilder<TDocument> : IWorkflowBuilder
    where TDocument : class, IWorkflowDocument
{
    /// <summary>Sets the stable definition identifier.</summary>
    IWorkflowBuilder<TDocument> Id(string id);

    /// <summary>Sets the definition version. Required to change on any structural edit.</summary>
    IWorkflowBuilder<TDocument> Version(int version);

    /// <summary>Sets the human-readable definition name.</summary>
    IWorkflowBuilder<TDocument> Name(string name);

    /// <summary>Begins the graph.</summary>
    IStartNodeBuilder<TDocument> Start();

    /// <summary>Produces the typed, immutable definition.</summary>
    new IProcessDefinition<TDocument> Build();
}
