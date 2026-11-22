Bpmn.Workflow.Engine — Assembly README
Purpose. Reference for implementing and extending the engine. Written for an AI agent or developer about to generate code. It lists every class and interface the engine must ship, its responsibility, its collaborators, and the invariants it must uphold. Read this together with the solution-level README (§3, §4). If a class below is missing or its behavior contradicts a stated invariant, the implementation is wrong.

Assembly: Bpmn.Workflow.Engine

Package ID: Bpmn.Workflow.Engine

Root namespace: Bpmn.Workflow.Core (this assembly is the only one under .Core; the Bpmn.Workflow.* root is owned by Bpmn.Workflow.Abstractions)

Depends on: Bpmn.Workflow.Abstractions only

Must not reference: EF Core, NServiceBus, Bpmn.Workflow.Persistence, Bpmn.Workflow.Runtime, Bpmn.Workflow.Api, any broker SDK, any host-specific package

Packable: yes

Public API: opt-in — most types are internal; the fluent builder implementations are public sealed (needed by consumers authoring workflows), everything else defaults to internal

Table of contents
Responsibility of the assembly

Folder layout

Public surface (what consumers see)

Internal surface (what the engine and runtime use)

Class catalog

Fluent — builders
Model — frozen graph
Expressions
Validation
Hashing
Handlers — built-in flow objects
Registry
Implementation order

Behavioral contracts per class

Invariants this assembly enforces

Testing obligations

Anti-patterns (reject these)

1. Responsibility of the assembly
Bpmn.Workflow.Engine owns three concerns:

Authoring implementation — implements the fluent DSL declared in Bpmn.Workflow.Abstractions and produces frozen IProcessDefinition<T> graphs.

Graph guarantees — validates graphs, computes the deterministic definition hash, freezes graphs, and provides the in-memory definition registry.

Built-in node behavior — implements IFlowNodeHandler for every built-in FlowNodeType (start/end, gateways, tasks, catch events, subprocess, update-document).

What it does not own: token persistence, message dispatch, document retrieval at run time (that's the provider), definition discovery at startup, or the execution loop itself. Those belong to Bpmn.Workflow.Runtime / Bpmn.Workflow.Persistence. The engine supplies the pieces; the runtime orchestrates them.

2. Folder layout
text
src/Bpmn.Workflow.Engine/
├── Fluent/
│   ├── WorkflowBuilder.cs
│   ├── WorkflowBuilderOfT.cs
│   ├── FlowBuilder.cs
│   ├── ExpressionCapture.cs
│   └── Nodes/
│       ├── StartNodeBuilder.cs
│       ├── EndNodeBuilder.cs
│       ├── ServiceTaskBuilder.cs
│       ├── UserTaskBuilder.cs
│       ├── ExclusiveGatewayBuilder.cs
│       ├── ParallelGatewayBuilder.cs
│       ├── InclusiveGatewayBuilder.cs
│       ├── TimerCatchEventBuilder.cs
│       ├── MessageCatchEventBuilder.cs
│       ├── UpdateDocumentBuilder.cs
│       └── SubProcessBuilder.cs
├── Model/
│   ├── ProcessDefinition.cs
│   ├── ProcessDefinitionOfT.cs
│   ├── FlowNodeModel.cs
│   ├── StartEventModel.cs
│   ├── EndEventModel.cs
│   ├── ServiceTaskModel.cs
│   ├── UserTaskModel.cs
│   ├── GatewayModel.cs
│   ├── BranchModel.cs
│   ├── TimerCatchEventModel.cs
│   ├── MessageCatchEventModel.cs
│   ├── UpdateDocumentTaskModel.cs
│   └── SubProcessModel.cs
├── Expressions/
│   ├── CapturedExpression.cs
│   ├── ExpressionRegistry.cs
│   └── ExpressionIds.cs
├── Validation/
│   ├── DefinitionGraphValidator.cs
│   ├── HandlerRegistrationValidator.cs
│   ├── ProviderRegistrationValidator.cs
│   └── DocumentTypeValidator.cs
├── Hashing/
│   ├── DefinitionHasher.cs
│   └── HashCanonicalizer.cs
├── Handlers/
│   ├── StartEventHandler.cs
│   ├── EndEventHandler.cs
│   ├── ExclusiveGatewayHandler.cs
│   ├── ParallelGatewayHandler.cs
│   ├── InclusiveGatewayHandler.cs
│   ├── ServiceTaskHandler.cs
│   ├── UserTaskHandler.cs
│   ├── TimerCatchEventHandler.cs
│   ├── MessageCatchEventHandler.cs
│   ├── SubProcessHandler.cs
│   └── UpdateDocumentHandler.cs
├── Registry/
│   ├── FlowNodeHandlerRegistry.cs
│   └── WorkflowDefinitionRegistry.cs
└── DependencyInjection/
    └── EngineServiceCollectionExtensions.cs
3. Public surface
These types must be public. Everything else defaults to internal.

3.1 Fluent builder entry (public sealed)
Type	Purpose
WorkflowBuilder<T>	Implements IWorkflowBuilder<T>. Created via DI extension AddWorkflowEngine(); developers receive it through IWorkflowDefinition<T>.Build.
WorkflowBuilder	Untyped adapter over WorkflowBuilder<T> for assembly scanning.
FlowBuilder<T>	The IStartNodeBuilder<T> implementation — factory for node builders.
3.2 Registration extension (public static)
Type	Purpose
EngineServiceCollectionExtensions	AddWorkflowEngine() — registers IWorkflowBuilder<T> (open generic), the handler registry, validators, hasher, definition registry.
3.3 Built-in handlers (public sealed)
Every built-in IFlowNodeHandler implementation is public so consumers can inspect or replace individual ones (e.g. substitute UserTaskHandler to change assignee materialization). They are registered by AddBuiltInFlowObjects() (see §3.2).

4. Internal surface
The following are internal — they are implementation details of the engine and are not exposed to consumers or the runtime. If the runtime needs something from this list, the seam is missing from Abstractions or Runtime — add it there, not by making the type public.

All *Builder node implementations.

All *Model frozen graph types.

ProcessDefinition, ProcessDefinition<T>.

CapturedExpression, ExpressionRegistry, ExpressionIds.

All validators, the hasher, the canonicalizer.

FlowNodeHandlerRegistry, WorkflowDefinitionRegistry implementations.

5. Class catalog
5.1 Fluent — builders
One file per node kind. Each builder:

Implements the corresponding interface from Bpmn.Workflow.Definitions.Fluent.

Returns the next builder in the chain from terminal methods (Next, Then, Body, OnRefreshed, OnNotFound).

Accumulates a FlowNodeDraft (internal intermediate) into the parent WorkflowBuilder<T>.

Captures lambdas into CapturedExpression with a stable ConditionId (or AssigneesExpressionId, etc.) that participates in the hash — never a delegate body.

Class	Implements	Notes
StartNodeBuilder<T>	IStartNodeBuilder<T>	Entry point. Builds all node kinds. Handles nested scopes (subprocess body, branch body).
EndNodeBuilder<T>	IEndEvent	Terminal. Does not implement a builder interface — created directly by .End() on the builder chain.
ServiceTaskBuilder<T>	IServiceTaskBuilder<T> + IServiceTaskOptions<T>	Collects HandlerDescriptor (type or key). One of the two is required at Build().
UserTaskBuilder<T>	IUserTaskBuilder<T> + IUserTaskOptions<T>	Collects AssigneesExpressionId. .Assignees(...) and .Assignee(...) both compile to a single assignees expression; the singular overload wraps the returned string in a one-element array.
ExclusiveGatewayBuilder<T>	IExclusiveGatewayBuilder<T>	Collects IBranchDefinition per .Branch(...). Requires at least 2 branches and exactly one default (first match wins; documented in §7).
ParallelGatewayBuilder<T>	IParallelGatewayBuilder<T>	Branches have no condition; model uses ConditionId = null.
InclusiveGatewayBuilder<T>	IInclusiveGatewayBuilder<T>	Branches have conditions like exclusive.
TimerCatchEventBuilder<T>	ITimerCatchEventBuilder<T> + ITimerCatchEventOptions	Requires exactly one of DueAt or Duration.
MessageCatchEventBuilder<T>	IMessageCatchEventBuilder<T> + IMessageCatchEventOptions<T>	Requires a correlation key expression.
UpdateDocumentBuilder<T>	IUpdateDocumentBuilder<T> + IUpdateDocumentOptions<T>	Requires .Provider<TProvider>() or .ProviderKey(...). OnRefreshed is required; OnNotFound optional.
SubProcessBuilder<T>	ISubProcessBuilder<T>	Manages a nested scope. Body tokens live in an inner dictionary at freeze time.
Structural note: builders are append-only. Once .Build() is called on the root WorkflowBuilder<T>, no builder can be reused. Build() is idempotent only in the sense that it returns the same frozen instance on repeated calls; it does not permit further mutation.

5.2 Model — frozen graph
Every model type is an internal sealed record (or class with init-only properties). They are frozen: no setters, no collections returned mutable, no delegates directly exposed except via CapturedExpression.

Type	Implements	Purpose
ProcessDefinition	IProcessDefinition	Root frozen object. Holds Start, Nodes (frozen dictionary), DefinitionHash, DocumentType.
ProcessDefinition<T>	IProcessDefinition<T>	Typed view; wraps a ProcessDefinition.
FlowNodeModel	IFlowNode	Base for all nodes. Holds Id, Name, Type, Incoming, Outgoing (frozen).
StartEventModel, EndEventModel	IStartEvent, IEndEvent	Terminal markers.
ServiceTaskModel	IServiceTask	Holds HandlerDescriptor.
UserTaskModel	IUserTask	Holds AssigneesExpressionId.
GatewayModel	IGateway	Holds IReadOnlyList<BranchModel>. Shared by exclusive/parallel/inclusive; FlowNodeType distinguishes them.
BranchModel	IBranchDefinition	Holds BranchId, TargetNodeId, ConditionId.
TimerCatchEventModel, MessageCatchEventModel	ITimerCatchEvent, IMessageCatchEvent	Wait states.
UpdateDocumentTaskModel	IUpdateDocumentTask	Holds Provider descriptor, NextNodeId, NotFoundNodeId.
SubProcessModel	IFlowNode + internal scope fields	Nested node ids (internal, not on the public interface).
5.3 Expressions
The engine must never freeze delegates into the graph. Lambdas are captured as CapturedExpression records and registered in an ExpressionRegistry (an internal service the runtime resolves when evaluating conditions/assignees).

Class	Purpose
CapturedExpression	internal sealed record: (string Id, ExpressionKind Kind, Delegate Body, int Arity). Kind is Condition, Assignees, CorrelationKey, Assignee, DueAt, Duration. Body is the typed delegate; the runtime casts it to the expected signature.
ExpressionRegistry	internal service; holds Id → CapturedExpression. Populated at Build() time; read-only afterward. Scoped to the definition; a graph never references an expression it didn't register.
ExpressionIds	internal static factory: Condition(scope, userSuppliedId), Assignees(scope, userSuppliedId). Constructs canonical IDs so identical user-supplied ids in different scopes don't collide.
Rule: every expression the developer supplies has an explicit id in the DSL (conditionId, assignee expression id, correlation key id). Those ids are the only thing that participates in the hash. The body is runtime-only.

5.4 Validation
Validators run inside Build() (fail-fast at authoring time) and again at startup (StartupDefinitionValidator in Bpmn.Workflow.Runtime calls into these). They are internal services injected into WorkflowBuilder<T>.

Class	Responsibility
DefinitionGraphValidator	Single start; all nodes reachable from start; no dangling outgoing edges; every non-end node has at least one outgoing; every gateway has ≥2 branches; parallel branches have no conditions; inclusive/exclusive branches have conditions; subprocess bodies are well-formed; update-document has NextNodeId set and NotFoundNodeId (optional but if set, resolvable). Throws WorkflowDefinitionException with a path-identifiable message.
HandlerRegistrationValidator	For every ServiceTaskModel, the descriptor resolves in the FlowNodeHandlerRegistry (by type in DI or by key). Throws with the offending node id.
ProviderRegistrationValidator	For every UpdateDocumentTaskModel, the descriptor resolves as IDocumentStateProvider<TDocument> for the definition's TDocument. Throws with the offending node id.
DocumentTypeValidator	Confirms the definition's TDocument : IWorkflowDocument and that the IWorkflowDefinition<T> type parameter matches. Throws if not.
Startup validation is the only gate. Do not validate definitions lazily on first instance; invalid graphs must fail at host startup.

5.5 Hashing
Class	Responsibility
DefinitionHasher	Computes the deterministic hash. Public method string Compute(ProcessDefinition definition). Deterministic across runs, machines, and runtime orderings.
HashCanonicalizer	Produces a stable canonical string representation of the graph — sorted nodes by id, sorted edges, sorted branches, all descriptor values rendered as type:<FullName> or key:<string>, all expression refs rendered as <Kind>:<Id>. Hashes the canonical string (SHA-256, lowercase hex).
What is hashed (exhaustive):

Every node's Id, Name, Type.

Incoming / Outgoing ids (sorted).

HandlerDescriptor as type:<FullName> or key:<string>.

BranchDefinition as BranchId, TargetNodeId, ConditionId (sorted by BranchId).

For UpdateDocumentTaskModel: provider descriptor, NextNodeId, NotFoundNodeId.

For TimerCatchEventModel: Duration/DueAt values (as ISO-8601 strings, if static).

For MessageCatchEventModel: CorrelationKeyExpressionId.

For SubProcessModel: the nested node ids (sorted).

What is not hashed:

Lambda bodies (delegates cannot be hashed deterministically).

Time of build.

Registry order.

The ProcessDefinition instance identity.

Reflection ordering.

If you change what's hashed, you break every instance. Bump Version(int) in the user-visible definition, and add a hash-pinning test.

5.6 Handlers — built-in flow objects
Each handler is public sealed, implements IFlowNodeHandler, resolves dependencies from IServiceProvider via the injected IExecutionContext, and returns a NodeExecutionResult. The runtime (Bpmn.Workflow.Runtime) invokes them from WorkflowEngine; the engine itself does not execute tokens.

Handler	NodeType	Behavior
StartEventHandler	StartEvent	Returns NodeExecutionResult.Continue(outgoing...). No side effects; the runtime already persisted the start token.
EndEventHandler	EndEvent	Returns NodeExecutionResult.Completed().
ExclusiveGatewayHandler	ExclusiveGateway	Evaluates each branch's condition (resolved via ExpressionRegistry + IExecutionContext<T>) in branch order; returns Continue for the first match. If none matches, returns Faulted("exclusive-gateway-no-match").
ParallelGatewayHandler	ParallelGateway	On fork (1 incoming, ≥2 outgoing): returns Continue(all outgoing). On join (≥2 incoming, 1 outgoing): returns Continue(outgoing) only if all incoming branches have arrived; otherwise Waiting("parallel-join") and the token is parked by the runtime until the join completes. The handler does not own the token-counting state — that's a runtime concern (see §7).
InclusiveGatewayHandler	InclusiveGateway	Splits on all branches whose condition is true; rejoins when all active branches arrive. Semantics mirror parallel split/join but driven by conditions.
ServiceTaskHandler	ServiceTask	Resolves the developer's IFlowNodeHandler by descriptor from IExecutionContext.Services, invokes it with the same node + context, and returns its NodeExecutionResult unchanged. The built-in service task is a dispatcher, not a worker — worker behavior is developer-supplied.
UserTaskHandler	UserTask	Resolves AssigneesExpressionId via ExpressionRegistry, invokes the delegate with IExecutionContext<T>, materializes string[] (trim, drop null/empty, dedupe ordinal, sort ordinal). Empty → Faulted("user-task-no-assignees"). Non-empty → Waiting("user-task", correlationKey: taskId). Assignee set is persisted by the runtime on the token (see README §2.4 / invariants #13).
TimerCatchEventHandler	TimerCatchEvent	Computes the due time (from DueAt or Duration relative to IExecutionContext now, if static) and returns Waiting("timer", correlationKey: <dueInstantTicks>"). The runtime schedules the wake-up. Do not schedule timers in the engine — that couples it to a scheduler/broker.
MessageCatchEventHandler	MessageCatchEvent	Resolves CorrelationKeyExpressionId, invokes the delegate with IExecutionContext<T>, returns Waiting("message", correlationKey: <computed key>). The runtime persists the token with that correlation key and resumes when DeliverMessageCommand arrives.
SubProcessHandler	SubProcess	Enters the subprocess scope: returns Continue(entryBodyNodeId). The runtime tracks scope on the token (a scope stack). The handler is a pure graph transition; scope lifecycle lives in the runtime.
UpdateDocumentHandler	UpdateDocument	Resolves the developer's IDocumentStateProvider<T> from IExecutionContext.Services by descriptor. Calls GetDocumentAsync(document.DocumentId, ct). On success: invokes a runtime callback (via IExecutionContext extension) to swap the document snapshot and mark it Refreshed, then returns Continue(NextNodeId). On DocumentNotFoundException: returns Continue(NotFoundNodeId) if configured, else Faulted("document-not-found"). Any other exception bubbles → runtime retry policy. The document swap must be committed in the same transaction as the token advance — this is the runtime's responsibility; the handler signals it via a well-defined NodeExecutionResult extension (the runtime looks for a pending-document-swap marker set on the context).
Critical: the handlers are pure with respect to persistence. They never write to the store. They return a NodeExecutionResult and, for UpdateDocument and UserTask, optionally set a marker on the IExecutionContext (implemented by the runtime) that tells the runtime "this needs to be persisted atomically." The runtime is the transaction owner.

5.7 Registry
Class	Responsibility
FlowNodeHandlerRegistry	internal sealed. Maps FlowNodeType → IFlowNodeHandler. Populated by AddBuiltInFlowObjects(); ServiceTaskHandler resolves custom handlers from IServiceProvider at execution time, not from this registry.
WorkflowDefinitionRegistry	internal sealed. Holds all built definitions, keyed by (Id, Version). TryGet, Get (throws WorkflowDefinitionException if absent), and All. Exposed via IWorkflowDefinitionProvider (from Abstractions.Extensibility) — do not expose the registry type.
6. Implementation order
An AI agent implementing this assembly should proceed in this order:

Model (Model/*) — the frozen graph. Get the record types right first; everything else depends on them.

Expressions (Expressions/*) — CapturedExpression, ExpressionRegistry, ExpressionIds.

Fluent (Fluent/*) — the builders, starting with WorkflowBuilder<T> → FlowBuilder<T> → a trivial StartNodeBuilder<T> that only supports .End(). Get Build() working end-to-end with StartEvent/EndEvent before adding nodes.

Hashing (Hashing/*) — canonicalizer + hasher. Pin a hash for a minimal graph as a test.

Validation (Validation/*) — graph validator first, then registration validators, then document type.

Handlers (Handlers/*) — start with StartEventHandler/EndEventHandler, then gateways, then tasks, then catch events, then UpdateDocumentHandler, then SubProcessHandler.

Registry (Registry/*) — handler registry + definition registry.

DI (DependencyInjection/EngineServiceCollectionExtensions.cs) — AddWorkflowEngine() and AddBuiltInFlowObjects().

Each step should ship with the corresponding tests (see §9). Do not proceed to the next step until the previous one compiles and its tests pass.

7. Behavioral contracts per class
WorkflowBuilder<T>
Id, Version, Name, Start, Build.

Build() is callable exactly once meaningfully; repeated calls return the same frozen instance.

At Build():

Calls each validator in order: DocumentTypeValidator → DefinitionGraphValidator → HandlerRegistrationValidator → ProviderRegistrationValidator.

Freezes the graph into ProcessDefinition<T>.

Computes and stamps DefinitionHash.

Registers all captured expressions in the injected ExpressionRegistry.

Throws WorkflowDefinitionException on the first validation failure with a message that identifies the offending node/branch.

DefinitionGraphValidator
Validates every invariant the graph model assumes. It must not look up DI (that's HandlerRegistrationValidator / ProviderRegistrationValidator). Its inputs are the draft graph and the document type. It runs before the registration validators.

DefinitionHasher
Pure function of the frozen graph.

Uses HashCanonicalizer to produce a canonical string, then SHA-256.

Returns lowercase hex.

Must be stable across net8.0 minor versions — do not include framework version or assembly version in the canonical string.

ExclusiveGatewayHandler
Iterates branches in declaration order, skipping branches with ConditionId == null (they can't match).

Resolves each condition via ExpressionRegistry.Get(conditionId).

First match wins → Continue(target).

No match → Faulted("exclusive-gateway-no-match"). Never silently default to the first branch — that hides authoring bugs.

ParallelGatewayHandler
A single handler serves both fork and join; the graph shape disambiguates (Incoming.Count / Outgoing.Count).

Fork: Continue(all outgoing).

Join: returns Continue(outgoing) only when the runtime signals all incoming tokens have arrived. The engine returns a dedicated marker that tells the runtime "this is a join, ask me again when the count is complete." Concretely, the handler returns Waiting("parallel-join") and the runtime parks the token in a join-completion list. When the count matches, the runtime re-invokes the handler with the join satisfied and it returns Continue(outgoing).

The engine does not persist join state. The runtime owns the "how many incoming branches have arrived" counter in the Token/ActivityRecord tables. The engine simply answers "is this join satisfied yet?" on each invocation.

UpdateDocumentHandler
Never mutates IExecutionContext.Document. It requests a swap via a runtime-provided callback on the context.

The callback is the runtime's way to record "swap document snapshot + advance token atomically."

If DocumentNotFoundException is caught: if NotFoundNodeId is set, requests a normal Continue to it; otherwise Faulted.

Does not cache anything. The snapshot cache is the persisted DocumentJson on the instance (persistence layer); the handler only fetches.

UserTaskHandler
Assignee expression is resolved once on entry. Do not store the expression in the token; store the resolved string[].

Empty set is a fault, not a Waiting. An empty user task is a definition bug, not an execution condition.

Task id (for correlationKey) is generated by the handler as a stable hash of (InstanceId, TokenId, NodeId). The runtime persists it.

8. Invariants this assembly enforces
The engine must uphold the solution-wide invariants (see README §4). The ones with direct implementation impact in this assembly:

#2 Definitions are immutable after Build(). All *Model types are record with init-only properties; Nodes is a frozen dictionary; Branches is IReadOnlyList. Never expose a mutable collection.

#3 Deterministic hash. Only structural data + explicit ids. Never delegates. Pin with tests.

#6 Wait-state persistence includes the snapshot. The engine's job here is to signal it, not perform it: UpdateDocumentHandler sets a swap marker; the runtime persists atomically.

#7 IDocumentStateProvider<T> called only at UpdateDocument. The engine must not fetch documents in any other handler.

#13 User-task assignees snapshotted on entry. The handler materializes the set; the runtime persists it; completion never re-evaluates.

#16 Transport-agnostic. No using NServiceBus.* — not even transitively.

#18 Message contracts live in Bpmn.Workflow.Messages. The engine does not publish or subscribe; it just returns results.

9. Testing obligations
For every class listed above, the engine must have:

Unit tests for the fluent builders: Build() produces the expected ProcessDefinition shape; validation failures produce specific WorkflowDefinitionException messages.

Unit tests for the hash: a fixture graph pins a specific hash. Changing the hash requires a deliberate change to the pinned value in the test — that's the guard against silent hash drift.

Unit tests for each validator: one passing case, one failing case per rule, asserting the failure message identifies the offending node/branch.

Unit tests for each handler: given a FakeExecutionContext (implements IExecutionContext<T>), assert the exact NodeExecutionResult. For UpdateDocumentHandler, assert the swap marker is set / not set. For UserTaskHandler, assert the materialized set's order and deduplication.

No integration tests in this assembly. All integration tests live in Bpmn.Workflow.Runtime.Tests and Bpmn.Workflow.Tests (per the solution README).

Architecture test: Bpmn.Workflow.Engine must not reference NServiceBus, EF Core, Bpmn.Workflow.Persistence, Bpmn.Workflow.Runtime, or Bpmn.Workflow.Api.

10. Anti-patterns
Do not do any of the following. These are the failure modes an AI agent is most likely to introduce:

Putting a delegate into a *Model type. Delegates are runtime-only. Capture lambdas in CapturedExpression, store only the id in the model.

Validating lazily. Validation must happen in Build() / startup. A definition that builds but throws on first execution is a bug in DefinitionGraphValidator.

Defaulting an exclusive gateway when no condition matches. Always Faulted. Defaults hide authoring bugs and cause silent production divergence.

Adding persistence code to a handler. Handlers never write to the store. If a handler seems to need persistence, the runtime is missing a transaction hook; extend the runtime, not the engine.

Scheduling timers from TimerCatchEventHandler. Timers are a runtime concern (the runtime's scheduling strategy depends on the transport). The handler returns Waiting with a due-instant correlation key.

Holding IServiceProvider in the graph. Handlers resolve services at execution time via IExecutionContext.Services; the graph is static and shared across instances.

Hashing delegates or reflection order. Only structural data + ids. Hashing must be deterministic across processes.

Making anything public except what §3 lists. The runtime and engine evolve together; only the composition surface is public. If the runtime needs something, add a contract to Abstractions or a seam to Runtime, not a public in the engine.

Returning multiple NodeExecutionResult types. One shape. No ConditionalNodeResult, no ParallelNodeResult. Extend NodeExecutionResult only if genuinely general.

Executing tokens inside a handler. A handler executes one node; the runtime drives the loop. A handler that loops over tokens is a runtime concern leaking upward.

Touching IExecutionContext.Document directly. Documents are swapped only by the runtime on UpdateDocumentHandler's request. The handler signals; it does not swap.

Summary
Bpmn.Workflow.Engine is a pure library that implements the fluent DSL, produces immutable hashed graphs, and provides one IFlowNodeHandler per built-in node kind. It has no persistence, no transport, no scheduler, and no execution loop — those live in Bpmn.Workflow.Runtime and Bpmn.Workflow.Persistence.

Structure: 7 folders, ~40 classes. Public surface: 3 fluent builder types + 11 built-in handlers + 1 DI extension. Everything else internal. Dependencies: Bpmn.Workflow.Abstractions only.

If an implementation choice is not covered here, the tie-breaker is the solution README's invariants and the engine's own test suite. Do not introduce a new pattern to solve a local problem; the engine's job is to be boringly predictable.