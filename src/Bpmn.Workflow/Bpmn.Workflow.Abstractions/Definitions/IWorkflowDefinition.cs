namespace Bpmn.Workflow.Definitions;

/// <summary>
/// Untyped authoring marker. Implementations are discovered by assembly scanning at
/// runtime startup and built into <see cref="IProcessDefinition"/> instances.
/// </summary>
public interface IWorkflowDefinition
{
    /// <summary>Builds the process definition using the fluent DSL.</summary>
    void Build(Fluent.IWorkflowBuilder builder);
}

/// <summary>
/// Typed authoring contract implemented by developers. Every workflow declares its
/// document type; the engine validates the pairing at startup.
/// </summary>
public interface IWorkflowDefinition<TDocument> : IWorkflowDefinition
    where TDocument : class, Documents.IWorkflowDocument
{
    /// <summary>Builds the process definition using the typed fluent DSL.</summary>
    void Build(Fluent.IWorkflowBuilder<TDocument> builder);
}
