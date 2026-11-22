Bpmn.Workflow.Abstractions — Assembly Reference
Complete reference for the abstractions assembly: purpose, dependency posture, full file/type inventory, and per-type contract details.

Assembly name: Bpmn.Workflow.Abstractions

Package ID: Bpmn.Workflow.Abstractions

Target framework: net8.0

Dependencies: none (BCL only)

Root namespace: Bpmn.Workflow

Packable: yes

Public API stability: enforced by Microsoft.CodeAnalysis.PublicApiAnalyzers

Table of contents
Purpose and scope

Structural rules

Namespace map

File inventory

Type catalog

Cross-cutting contracts

What is intentionally not here

Versioning rules

1. Purpose and scope
Bpmn.Workflow.Abstractions is the public contract surface of the workflow library. It is the only assembly a consumer must reference to author workflows. It contains:

Authoring contracts developers implement (IWorkflowDefinition<T>, IWorkflowBuilder<T>, IDocumentStateProvider<T>, IFlowNodeHandler, …).

The frozen graph model (IProcessDefinition, IFlowNode, per-node interfaces).

Execution contracts the engine invokes (IExecutionContext, NodeExecutionResult).

Extensibility hooks (IBehaviorExtension, IExpressionEvaluator, IWorkflowDefinitionProvider).

Document contracts (IWorkflowDocument, IDocumentStateProvider<T>, IDocumentMigrator<T>, DocumentNotFoundException).

Exceptions co-located with the feature that produces them.

It does not contain: any implementation, any reference to Bpmn.Workflow.Engine, Bpmn.Workflow.Persistence, Bpmn.Workflow.Runtime, Bpmn.Workflow.Api, or any broker SDK.

2. Structural rules
The assembly follows the solution-wide conventions:

Feature-driven namespaces. Types live with the feature that owns them, not in generic Exceptions/, Enums/, Contracts/ folders.

Exceptions live next to (or in the root of) the contract that produces them. WorkflowDefinitionException is at root because both the fluent builder and the startup validator throw it; DefinitionDriftException is in Definitions; DocumentTypeMismatchException is in Execution.

Enums live with their consumer. FlowNodeType is in FlowObjects (it describes the graph), ExecutionStatus is in Execution (it describes the return of IFlowNodeHandler).

No implementation types. Every type is an interface, a record, an enum, or an exception. There are no handlers, builders, stores, or services.

No DI container types beyond IServiceProvider. The execution context exposes IServiceProvider (BCL) — nothing else references DI.

3. Namespace map
Namespace	Purpose
Bpmn.Workflow	Root — shared exceptions
Bpmn.Workflow.Documents	Typed-document feature (document, provider, migrator, not-found)
Bpmn.Workflow.Definitions	Process definition model + authoring contract + drift exception
Bpmn.Workflow.Definitions.Fluent	Fluent DSL (authoring surface; produces IProcessDefinition<T>)
Bpmn.Workflow.FlowObjects	Frozen graph node model, node kind enum, handler descriptor
Bpmn.Workflow.Execution	Engine execution contracts (handler, context, result, execution exceptions)
Bpmn.Workflow.Extensibility	Pluggable hooks (behavior, expression evaluator, definition provider)
4. File inventory
File (under Bpmn.Workflow.Abstractions/)	Namespace	Types
WorkflowDefinitionException.cs	Bpmn.Workflow	WorkflowDefinitionException
Documents/IWorkflowDocument.cs	Bpmn.Workflow.Documents	IWorkflowDocument
Documents/IDocumentStateProvider.cs	Bpmn.Workflow.Documents	IDocumentStateProvider, IDocumentStateProvider<T>
Documents/DocumentNotFoundException.cs	Bpmn.Workflow.Documents	DocumentNotFoundException
Documents/IDocumentMigrator.cs	Bpmn.Workflow.Documents	IDocumentMigrator<T>
Definitions/IProcessDefinition.cs	Bpmn.Workflow.Definitions	IProcessDefinition, IProcessDefinition<T>
Definitions/IWorkflowDefinition.cs	Bpmn.Workflow.Definitions	IWorkflowDefinition, IWorkflowDefinition<T>
Definitions/DefinitionDriftException.cs	Bpmn.Workflow.Definitions	DefinitionDriftException
Definitions/Fluent/IWorkflowBuilder.cs	Bpmn.Workflow.Definitions.Fluent	IWorkflowBuilder, IWorkflowBuilder<T>
Definitions/Fluent/IStartNodeBuilder.cs	Bpmn.Workflow.Definitions.Fluent	IStartNodeBuilder<T>
Definitions/Fluent/IActivityBuilder.cs	Bpmn.Workflow.Definitions.Fluent	IActivityBuilder<T>
Definitions/Fluent/IServiceTaskBuilder.cs	Bpmn.Workflow.Definitions.Fluent	IServiceTaskBuilder<T>, IServiceTaskOptions<T>
Definitions/Fluent/IUserTaskBuilder.cs	Bpmn.Workflow.Definitions.Fluent	IUserTaskBuilder<T>, IUserTaskOptions<T>
Definitions/Fluent/IGatewayBuilders.cs	Bpmn.Workflow.Definitions.Fluent	IExclusiveGatewayBuilder<T>, IInclusiveGatewayBuilder<T>, IParallelGatewayBuilder<T>, IBranchBuilder<T>, IBranchOptions<T>
Definitions/Fluent/ICatchEventBuilders.cs	Bpmn.Workflow.Definitions.Fluent	ITimerCatchEventBuilder<T>, ITimerCatchEventOptions, IMessageCatchEventBuilder<T>, IMessageCatchEventOptions<T>
Definitions/Fluent/IUpdateDocumentBuilder.cs	Bpmn.Workflow.Definitions.Fluent	IUpdateDocumentBuilder<T>, IUpdateDocumentOptions<T>
Definitions/Fluent/ISubProcessBuilder.cs	Bpmn.Workflow.Definitions.Fluent	ISubProcessBuilder<T>
FlowObjects/IFlowNode.cs	Bpmn.Workflow.FlowObjects	IFlowNode, FlowNodeType, IStartEvent, IEndEvent
FlowObjects/HandlerDescriptor.cs	Bpmn.Workflow.FlowObjects	HandlerDescriptor
FlowObjects/IActivity.cs	Bpmn.Workflow.FlowObjects	IActivity, IServiceTask, IUserTask
FlowObjects/IGateway.cs	Bpmn.Workflow.FlowObjects	IGateway, IBranchDefinition
FlowObjects/IUpdateDocumentTask.cs	Bpmn.Workflow.FlowObjects	IUpdateDocumentTask
FlowObjects/ICatchEvents.cs	Bpmn.Workflow.FlowObjects	ITimerCatchEvent, IMessageCatchEvent
Execution/IFlowNodeHandler.cs	Bpmn.Workflow.Execution	IFlowNodeHandler
Execution/IExecutionContext.cs	Bpmn.Workflow.Execution	IExecutionContext, IExecutionContext<T>
Execution/NodeExecutionResult.cs	Bpmn.Workflow.Execution	ExecutionStatus, NodeExecutionResult
Execution/ExecutionExceptions.cs	Bpmn.Workflow.Execution	WorkflowExecutionException, DocumentTypeMismatchException
Extensibility/IBehaviorExtension.cs	Bpmn.Workflow.Extensibility	IBehaviorExtension
Extensibility/IExpressionEvaluator.cs	Bpmn.Workflow.Extensibility	IExpressionEvaluator
Extensibility/IWorkflowDefinitionProvider.cs	Bpmn.Workflow.Extensibility	IWorkflowDefinitionProvider
Totals: 30 files, 47 types (29 interfaces + 2 abstract-base exceptions + 5 sealed exceptions + 4 enums + 3 records + 4 derived interfaces counted as separate — see catalog below).

5. Type catalog
5.1 Interfaces (authoring and contract)
Interface	Namespace	Purpose	Implemented by
IWorkflowDefinition	Definitions	Untyped authoring marker (discovery via assembly scan).	Infrastructure adapters over IWorkflowDefinition<T>.
IWorkflowDefinition<T>	Definitions	Developer implements this. Builds the process definition via the fluent DSL.	Developer workflow classes.
IWorkflowBuilder	Definitions.Fluent	Untyped builder surface (Build()).	Infrastructure.
IWorkflowBuilder<T>	Definitions.Fluent	Typed root DSL: .Id(), .Version(), .Name(), .Start(), .Build().	Developer (via IWorkflowDefinition<T>.Build).
IStartNodeBuilder<T>	Definitions.Fluent	Entry point for a sequence; exposes node kinds.	Fluent chain (returned by Start() / Next() / Then() / Body() / OnRefreshed() / OnNotFound()).
IActivityBuilder<T>	Definitions.Fluent	Chaining surface for activity nodes (.Next()).	IServiceTaskBuilder<T>, IUserTaskBuilder<T>.
IServiceTaskBuilder<T>, IServiceTaskOptions<T>	Definitions.Fluent	Service task configuration (.Handler<T>(), .HandlerKey()).	Developer.
IUserTaskBuilder<T>, IUserTaskOptions<T>	Definitions.Fluent	User task configuration (.Assignees(...), .Assignee(...)).	Developer.
IExclusiveGatewayBuilder<T>, IInclusiveGatewayBuilder<T>, IParallelGatewayBuilder<T>	Definitions.Fluent	Gateway branch configuration.	Developer.
IBranchBuilder<T>, IBranchOptions<T>	Definitions.Fluent	.Then() and .Condition(conditionId, predicate).	Developer.
ITimerCatchEventBuilder<T>, ITimerCatchEventOptions	Definitions.Fluent	Timer configuration (.DueAt(), .Duration()).	Developer.
IMessageCatchEventBuilder<T>, IMessageCatchEventOptions<T>	Definitions.Fluent	Message configuration (.CorrelationKey(selector)).	Developer.
IUpdateDocumentBuilder<T>, IUpdateDocumentOptions<T>	Definitions.Fluent	Refresh configuration (.OnRefreshed, .OnNotFound, .Provider<T>, .ProviderKey).	Developer.
ISubProcessBuilder<T>	Definitions.Fluent	.Body() and .Next().	Developer.
IProcessDefinition	Definitions	Untyped frozen definition (engine-facing).	Engine / runtime.
IProcessDefinition<T>	Definitions	Typed view of a frozen definition.	Engine / runtime.
IDocumentStateProvider	Documents	Untyped marker for discovery.	Runtime registration.
IDocumentStateProvider<T>	Documents	Developer implements this. Pulls the authoritative document state by DocumentId.	Developer.
IDocumentMigrator<T>	Documents	Optional schema-drift migrator for persisted snapshots.	Developer (optional).
IWorkflowDocument	Documents	Marker for typed document POCOs; exposes DocumentId.	Developer document classes.
IFlowNode	FlowObjects	Base node shape (Id, Name, Type, Incoming, Outgoing).	All node kinds.
IStartEvent, IEndEvent	FlowObjects	Marker node kinds.	Frozen graph.
IActivity, IServiceTask	FlowObjects	Executable node shape with a HandlerDescriptor.	Frozen graph.
IUserTask	FlowObjects	User task shape (AssigneesExpressionId).	Frozen graph.
IGateway, IBranchDefinition	FlowObjects	Gateway branches (BranchId, TargetNodeId, ConditionId).	Frozen graph.
IUpdateDocumentTask	FlowObjects	Refresh node shape (Provider, NextNodeId, NotFoundNodeId).	Frozen graph.
ITimerCatchEvent, IMessageCatchEvent	FlowObjects	Wait state shapes.	Frozen graph.
IFlowNodeHandler	Execution	Implemented for built-in nodes; can be extended. Executes a node.	Engine / developer (custom).
IExecutionContext	Execution	Untyped execution context (Document as object, GetDocument<T>()).	Engine.
IExecutionContext<T>	Execution	Typed context for conditions/expressions (Document as T).	Engine.
IBehaviorExtension	Extensibility	Cross-cutting OnEnterAsync / OnExitAsync hooks.	Developer (optional).
IExpressionEvaluator	Extensibility	Evaluates data-expressed conditions.	Developer (optional).
IWorkflowDefinitionProvider	Extensibility	Lookup of definitions by (id, version).	Engine (default), custom providers.
5.2 Records
Record	Namespace	Purpose
HandlerDescriptor(Type? HandlerType, string? HandlerKey)	FlowObjects	Type-or-key reference to a handler/provider. Never an instance. Factory: ByType, ByKey.
NodeExecutionResult(...)	Execution	The engine's single execution return shape. Factories: Continue, Waiting, Completed, Faulted.
5.3 Enums
Enum	Namespace	Values	Purpose
FlowNodeType	FlowObjects	StartEvent, EndEvent, ServiceTask, UserTask, UpdateDocument, ExclusiveGateway, ParallelGateway, InclusiveGateway, TimerCatchEvent, MessageCatchEvent, SubProcess	Node kind.
ExecutionStatus	Execution	Continue, Waiting, Completed, Faulted	How a node finished.
5.4 Exceptions
Exception	Namespace	Raised by
WorkflowDefinitionException	Bpmn.Workflow (root)	Fluent builder Build() and startup validator.
DefinitionDriftException	Definitions	Definition pin check on instance resume. Properties: DefinitionId, Version, ExpectedHash, ActualHash.
DocumentNotFoundException	Documents	IDocumentStateProvider<T> when the document is known not to exist. Property: DocumentId.
WorkflowExecutionException	Execution	Base for engine-pipeline failures.
DocumentTypeMismatchException	Execution	IExecutionContext.GetDocument<T>() on type mismatch. Properties: Expected, Actual.
5.5 Marker / builder types combined
The DSL surface is composed of 19 interfaces across 7 files in Definitions.Fluent. The split is by node kind, not by role: each builder returns the next builder in the chain, and each options interface is passed as an Action<> to the corresponding builder method. This keeps the authoring surface discoverable and compile-time-checkable while allowing new node kinds to be added without touching existing interfaces.

6. Cross-cutting contracts
These are the contracts every other assembly depends on. They must remain stable.

6.1 NodeExecutionResult
The single way a node reports "where to go next" and "am I waiting". Any node extension must use this shape — no side-channel result types.

csharp
public sealed record NodeExecutionResult(
    IReadOnlyList<string> NextNodeIds,
    ExecutionStatus Status,
    IReadOnlyDictionary<string, object?>? VariableUpdates = null,
    string? WaitReason = null,
    string? CorrelationKey = null,
    string? FaultReason = null);
6.2 IExecutionContext / IExecutionContext<T>
Untyped engine-facing context vs. typed context for expressions. Handlers that need the document call GetDocument<T>() (which throws DocumentTypeMismatchException on mismatch). Conditions/assignee expressions receive IExecutionContext<T> directly.

6.3 HandlerDescriptor
The descriptor is what participates in the definition hash. It never holds an instance. This is why definitions can be frozen and hashed; if the descriptor held a captured delegate, the hash would not be stable.

6.4 IProcessDefinition hash inputs
The engine computes a deterministic structural hash over: node ids, node types, edges, descriptors, and developer-supplied ConditionId strings (not the lambda bodies). ConditionId is the only string identifier in the DSL whose stability is part of the public contract. Changing a ConditionId requires bumping IProcessDefinition.Version.

7. What is intentionally not here
No IWorkflowInstanceStore / IDocumentSnapshotStore. Persistence contracts live in Bpmn.Workflow.Persistence. Putting the store interface in Abstractions would couple it to a consumer-facing package and force every package consumer to see persistence concerns.

No message contracts. StartProcessCommand, CompleteUserTaskCommand, integration events — all live in Bpmn.Workflow.Messages.

No NServiceBus types. No IHandleMessages<T>, no IMessageSession, no broker attributes.

No engine logic. No WorkflowEngine, no builders, no validators.

No ICommandPublisher / ICommandExecutor. Those are API seams and live in Bpmn.Workflow.Api.

No IWorkflowRuntimeHost / IWorkflowMessageDispatcher. Runtime seams live in Bpmn.Workflow.Runtime.

No FormKey, no Revision, no groups/roles. Removed by design decisions.

No IDocumentUpdater, DocumentUpdateRequest, DocumentUpdateResult<T>, DocumentUpdateInactivityPolicy, IDocumentAuthorizationPolicy<T>. Removed by the pull-based UpdateDocument redesign.

If a type you need is missing from this assembly, it belongs to a different one. The checklist for “where should a new type go”:

Does the engine need it by contract (invocation or type constraint)? → Bpmn.Workflow.Abstractions.

Is it a frozen-model shape consumed by handlers or conditions? → Bpmn.Workflow.Abstractions.

Is it an extension point the engine calls into? → Bpmn.Workflow.Abstractions.

Is it persistence-related? → Bpmn.Workflow.Persistence.

Is it transport-related? → adapter package.

Is it engine logic? → Bpmn.Workflow.Engine.

Is it a message / event? → Bpmn.Workflow.Messages.

Otherwise → the specific feature assembly.

8. Versioning rules
Semantic versioning applies to the package (Microsoft.CodeAnalysis.PublicApiAnalyzers + Nerdbank.GitVersioning).

Public API changes are tracked in PublicAPI.Shipped.txt / PublicAPI.Unshipped.txt. Removing or changing a public type is a breaking change — minor bump at minimum, major bump if the contract is load-bearing for authored graphs.

Graph shape changes (e.g., removing a member from an IFlowNode interface) are breaking for definitions: any definition that produced a hash with the old shape will raise DefinitionDriftException on resume. The Version(int) on IProcessDefinition is the user-visible mitigation — but consumers must bump it explicitly. Document this in release notes.

Namespace moves are breaking for consumers even when types are moved within the same assembly. Avoid moves; if forced, ship a type-forwarder for one version.

Summary
Bpmn.Workflow.Abstractions is a small, dependency-free contract surface. Every type is an interface, a record, an enum, or an exception. It contains the developer-facing authoring DSL, the frozen graph model, the execution contracts the engine relies on, the typed-document feature's contracts, and the extension hooks. It does not contain persistence, messaging, transport, engine logic, or API seams — those live in their respective assemblies.

30 files, 47 types, zero dependencies. If a change cannot be justified under §6 (cross-cutting) or §5 (feature contract), it belongs in a different assembly.

Root
https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Bpmn.Workflow.Abstractions.csproj

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/WorkflowDefinitionException.cs

Documents/
https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Documents/IWorkflowDocument.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Documents/IDocumentStateProvider.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Documents/DocumentNotFoundException.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Documents/IDocumentMigrator.cs

Definitions/
https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/IProcessDefinition.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/IWorkflowDefinition.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/DefinitionDriftException.cs

Definitions/Fluent/
https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/IWorkflowBuilder.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/IStartNodeBuilder.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/IActivityBuilder.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/IServiceTaskBuilder.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/IUserTaskBuilder.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/IGatewayBuilders.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/ICatchEventBuilders.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/IUpdateDocumentBuilder.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/ISubProcessBuilder.cs

FlowObjects/
https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/FlowObjects/IFlowNode.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/FlowObjects/HandlerDescriptor.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/FlowObjects/IActivity.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/FlowObjects/IGateway.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/FlowObjects/IUpdateDocumentTask.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/FlowObjects/ICatchEvents.cs

Execution/
https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Execution/IFlowNodeHandler.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Execution/IExecutionContext.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Execution/NodeExecutionResult.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Execution/ExecutionExceptions.cs

Extensibility/
https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Extensibility/IBehaviorExtension.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Extensibility/IExpressionEvaluator.cs

https://github.com/zykindmitry/bpmn.workflow/tree/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Extensibility/IWorkflowDefinitionProvider.cs

Same list, as fetchable blob/main URLs
To have me verify the contents against the proposed solution, send the blob/main form:

text
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Bpmn.Workflow.Abstractions.csproj
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/WorkflowDefinitionException.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Documents/IWorkflowDocument.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Documents/IDocumentStateProvider.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Documents/DocumentNotFoundException.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Documents/IDocumentMigrator.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/IProcessDefinition.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/IWorkflowDefinition.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/DefinitionDriftException.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/IWorkflowBuilder.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/IStartNodeBuilder.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/IActivityBuilder.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/IServiceTaskBuilder.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/IUserTaskBuilder.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/IGatewayBuilders.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/ICatchEventBuilders.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/IUpdateDocumentBuilder.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Definitions/Fluent/ISubProcessBuilder.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/FlowObjects/IFlowNode.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/FlowObjects/HandlerDescriptor.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/FlowObjects/IActivity.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/FlowObjects/IGateway.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/FlowObjects/IUpdateDocumentTask.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/FlowObjects/ICatchEvents.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/FlowObjects/IServiceTask.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/FlowObjects/IUserTask.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Execution/IFlowNodeHandler.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Execution/IExecutionContext.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Execution/NodeExecutionResult.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Execution/ExecutionExceptions.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Extensibility/IBehaviorExtension.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Extensibility/IExpressionEvaluator.cs
https://github.com/zykindmitry/bpmn.workflow/blob/main/src/Bpmn.Workflow/Bpmn.Workflow.Abstractions/Extensibility/IWorkflowDefinitionProvider.cs