# bpmn.workflow
Bpmn.Workflow — README
Purpose of this document. This README doubles as an LLM context primer for the codebase. It explains what the solution is, why it's shaped the way it is, and which invariants must not be broken. If you are an LLM being asked to contribute code, read the Design Concepts and Invariants sections first — they encode decisions that are easy to violate by accident.

1. What this is
A developer-first, fluent-API BPMN workflow library for .NET 8. Workflows are defined in C# code (not uploaded as XML), operate on a strongly-typed developer document, and execute on a NServiceBus-hosted, token-based engine with durable persistence across wait states.

The solution is packaged as five assemblies:

Assembly	Role
Bpmn.Workflow.Abstractions	Public contracts only. No dependencies. This is the surface developers code against.
Bpmn.Workflow.Engine	Fluent DSL implementation, built-in flow-object handlers, graph validation, definition hashing.
Bpmn.Workflow.Persistence	EF Core entities and stores for process instances, tokens, variables, activity audit, document snapshots.
Bpmn.Workflow.Service	NServiceBus endpoint host. Owns the runtime lifecycle: consumes commands/events, steps tokens, persists, resumes.
Bpmn.Workflow.Api	ASP.NET Core Web API. Read-only view of definitions; commands for start / complete-task / deliver-message / cancel; read-only document snapshot.
Who calls what:

text
Client ──HTTP──▶ Bpmn.Workflow.Api ──NServiceBus commands──▶ Bpmn.Workflow.Service
                                                                     │
                                                     ┌───────────────┼────────────────┐
                                                     ▼               ▼                ▼
                                            Bpmn.Workflow.Engine  Bpmn.Workflow.Persistence
                                            (graph + handlers)    (instances, snapshots)
The API never executes workflow logic and never touches persistence directly. It publishes commands and reads projections. This boundary is a hard rule.

2. Key features
2.1 Fluent-API workflow definitions (no XML)
Workflows are C# classes implementing IWorkflowDefinition<TDocument>:

csharp
public sealed class PurchaseOrderWorkflow : IWorkflowDefinition<PurchaseOrder>
{
    public void Build(IWorkflowBuilder<PurchaseOrder> b) => b
        .Id("purchase-order").Version(3).Name("Purchase Order Approval")
        .Start()
            .ServiceTask("validate", t => t.Handler<ValidateOrderHandler>())
            .ExclusiveGateway("amount-check")
                .Branch("high", x => x.Condition("high", ctx => ctx.Document.Amount > 10_000m))
                    .Then().UserTask("manager-approval")
                    .UpdateDocument("refresh", u => u.Provider<PurchaseOrderStateProvider>())
                        .OnRefreshed(n => n.End())
                        .OnNotFound(n => n.End())
                .Branch("low", x => x.Condition("low", ctx => ctx.Document.Amount <= 10_000m))
                    .Then().End()
        .Build();
}
Definitions are compiled artifacts that ship with the application. There is no upload endpoint.

The DSL produces an immutable IProcessDefinition via Build().

Graph validation and a deterministic structural hash are computed at build/startup. Invalid graphs fail fast at startup.

Definition versioning is explicit (Version(int)). Instances pin (Id, Version, Hash); drift raises DefinitionDriftException.

2.2 Typed workflow documents
Every workflow declares a document type:

csharp
public sealed class PurchaseOrder : IWorkflowDocument
{
    public string DocumentId { get; init; } = default!;
    public decimal Amount { get; init; }
    public string Department { get; init; } = default!;
    // ...
}
The document is the single source of truth for process data. It is persisted on the instance (DocumentJson).

Gateway conditions, user-task assignees, message correlation keys, etc. receive IExecutionContext<TDocument> and read ctx.Document.X with compile-time safety.

No revision field. The engine does not manage document concurrency. If a developer needs optimistic concurrency, they model it themselves and enforce it in their provider.

2.3 Pull-based UpdateDocument with snapshot caching
The document's authoritative state is owned outside the workflow. When a workflow needs a fresh copy, it executes an UpdateDocument node:

Engine resolves the developer's IDocumentStateProvider<TDocument> from DI.

Engine calls GetDocumentAsync(documentId, ct).

On success, the fetched document replaces the in-flight snapshot; every subsequent activity sees the new state.

On DocumentNotFoundException, the token routes to the configured OnNotFound branch (or faults if none).

On any other exception, it's treated as transient and the host retries.

Snapshot caching is the core performance property: at every wait state the instance is persisted with its current document snapshot. Resuming from a wait does not call the provider. The expensive retrieval only happens at explicit UpdateDocument nodes.

This is why the abstraction is IDocumentStateProvider<T> (pull) and not an "updater" (push): the workflow decides when to refresh, not the outside world.

2.4 NServiceBus-based runtime
The service is a real NServiceBus endpoint, not a console loop. You get:

Message retries, outbox semantics, transient-fault handling.

Correlation for wait states (user tasks, timers, messages).

Clean shutdown/restart: an instance resumes exactly where it persisted.

2.5 Token-based execution
Process instances hold one or more tokens. Each token is (CurrentNodeId, Status, WaitReason, CorrelationKey). This is the only model that correctly handles parallel gateways, inclusive gateways, and joins. The engine steps tokens; when a token reaches a wait state, the instance is persisted and the transaction completes.

2.6 Feature-driven namespaces
Namespaces mirror features, not layers. Exceptions live next to (or in the root of) the contracts that produce them:

Bpmn.Workflow — shared root (WorkflowDefinitionException).

Bpmn.Workflow.Documents — typed-document feature.

Bpmn.Workflow.Definitions — definition model; .Fluent sub-namespace for the DSL.

Bpmn.Workflow.FlowObjects — frozen graph model.

Bpmn.Workflow.Execution — engine execution contracts and execution exceptions.

Bpmn.Workflow.Extensibility — pluggable hooks.

3. Design concepts (the mental model)
3.1 Contract / implementation split
Bpmn.Workflow.Abstractions is the only assembly a consumer must reference. It contains zero dependencies beyond the BCL, zero implementation, and zero types from other Bpmn assemblies. If you find yourself wanting to add "a little logic" to it, stop — add it to Bpmn.Workflow.Engine and expose it via a contract.

3.2 Definitions are immutable and hashable
IProcessDefinition is frozen after Build(). The definition hash is computed over:

node ids and node types,

edges (incoming/outgoing),

handler/provider descriptors,

developer-supplied ConditionId strings (not the lambda bodies).

Consequence: refactoring a lambda body does not change the hash; changing a ConditionId does. Any structural edit must bump Version(int). Instances pin (Id, Version, Hash) and raise DefinitionDriftException on mismatch.

3.3 Documents are snapshots
An instance's document is a point-in-time snapshot, not a live handle. The authoritative state lives wherever the developer's IDocumentStateProvider<T> reads it from. Between UpdateDocument nodes the workflow sees the snapshot from the last fetch. This is intended: freshness is opt-in, and authors place refresh nodes where they need it.

3.4 Handlers are descriptors, not instances
HandlerDescriptor identifies a handler by type or key. It never holds an instance. This is what makes definitions freezable and hashable, and keeps DI resolution at runtime. Do not put instantiated handlers, delegates with captured state, or IServiceProvider references in the frozen graph.

3.5 NodeExecutionResult is the single engine contract
Every node — built-in or custom — returns this shape. It expresses three things:

Status (Continue | Waiting | Completed | Faulted),

NextNodeIds,

optional VariableUpdates, WaitReason, CorrelationKey, FaultReason.

If you find yourself wanting a special-case return type for a node, resist it. Extend NodeExecutionResult if genuinely general; otherwise model your custom behavior within these four statuses.

3.6 The API is a façade
The API:

publishes commands via NServiceBus,

reads projections (read models) for status,

lists definitions and returns a graph projection (read-only),

returns the persisted document snapshot (read-only; explicitly not the live source).

The API must not:

execute workflow logic,

reference Bpmn.Workflow.Persistence or Bpmn.Workflow.Engine,

mutate definitions,

mutate documents.

3.7 Namespace-to-assembly discipline
Assembly	Root namespace
Bpmn.Workflow.Abstractions	Bpmn.Workflow
Bpmn.Workflow.Engine	Bpmn.Workflow.Core
Bpmn.Workflow.Persistence	Bpmn.Workflow.Persistence
Bpmn.Workflow.Service	Bpmn.Workflow.Runtime
Bpmn.Workflow.Api	Bpmn.Workflow.Api
Bpmn.Workflow.Service keeps the namespace Bpmn.Workflow.Runtime (historical). No public contract in Abstractions may reference Bpmn.Workflow.Core.*. This is enforced by an architecture test in CI.

4. Invariants (things that must not be broken)
These are the non-negotiables. Most regressions in a workflow engine come from violating one of them.

Abstractions has no dependencies (BCL only) and exposes no implementation types.

Definitions are immutable after Build(). No mutation of IProcessDefinition or its nodes.

Definition hash must be deterministic. It's derived from structural data + explicit ids, never from delegate bodies, reflection order, or runtime state.

Instances pin to (Id, Version, Hash). Resume on a mismatch raises DefinitionDriftException. Never silently re-map to a newer definition.

The engine swaps the document only at UpdateDocument nodes. There is no implicit refresh — not on resume, not on wait-state transitions, not on retries.

Wait-state persistence includes the current document snapshot. A crash or restart must resume with the same snapshot the instance had at the moment of waiting.

IDocumentStateProvider<T> is called only when a token executes an UpdateDocument node. If a test asserts "provider was not called on resume," it must pass.

The API never references Persistence or Engine. All execution goes through NServiceBus commands.

NodeExecutionResult is the only execution return shape. No side-channel result types for specific nodes.

Externals are authoritative. The engine never authoritatively mutates the document. It replaces its snapshot with what the provider returned.

Namespace discipline holds. Bpmn.Workflow.Abstractions owns Bpmn.Workflow.*; the implementation assemblies never shadow that tree with their own public types.

Failure classification is explicit. DocumentNotFoundException = permanent → route to OnNotFound. Any other exception = transient → host retry. Do not blur this line.

5. Repository layout
text
Bpmn.Workflow.sln
├── src/
│   ├── Bpmn.Workflow.Abstractions/   (public contracts; BCL only)
│   ├── Bpmn.Workflow.Engine/         (fluent DSL + built-in handlers; ns Bpmn.Workflow.Core)
│   ├── Bpmn.Workflow.Persistence/    (EF Core; ns Bpmn.Workflow.Persistence)
│   ├── Bpmn.Workflow.Service/        (NServiceBus host; ns Bpmn.Workflow.Runtime)
│   └── Bpmn.Workflow.Api/            (ASP.NET Core; ns Bpmn.Workflow.Api)
├── tests/
│   ├── Bpmn.Workflow.Tests/                 (DSL, validation, hash, handler behavior)
│   ├── Bpmn.Workflow.Service.Tests/         (engine integration, restart, resume)
│   ├── Bpmn.Workflow.Api.Tests/             (WebApplicationFactory + fake IMessageSession)
│   └── Bpmn.Workflow.Persistence.Tests/     (Testcontainers)
├── samples/
│   ├── Bpmn.Sample.Definitions/     (PurchaseOrder + provider + workflow + handlers)
│   └── Bpmn.Sample.Host/            (docker-compose: DB, broker, service, api)
├── build/  (Directory.Build.props, version.json, ci/)
└── docs/   (architecture, fluent DSL, typed documents, extensions, supported elements)
Dependency rules (enforced by NetArchTest)
Project	May reference
Bpmn.Workflow.Abstractions	BCL only
Bpmn.Workflow.Engine	Abstractions
Bpmn.Workflow.Persistence	Abstractions
Bpmn.Workflow.Service	Abstractions, Engine, Persistence
Bpmn.Workflow.Api	Abstractions (+ messaging contracts) — not Persistence, not Engine
6. Using the library
6.1 Define a document
csharp
public sealed class PurchaseOrder : IWorkflowDocument
{
    public string DocumentId { get; init; } = default!;
    public decimal Amount { get; init; }
    public string Department { get; init; } = default!;
}
6.2 Implement the state provider
csharp
public sealed class PurchaseOrderStateProvider : IDocumentStateProvider<PurchaseOrder>
{
    private readonly IPurchaseOrderRepository _repo;
    public PurchaseOrderStateProvider(IPurchaseOrderRepository repo) => _repo = repo;

    public async Task<PurchaseOrder> GetDocumentAsync(string documentId, CancellationToken ct)
        => await _repo.LoadAsync(documentId, ct)
           ?? throw new DocumentNotFoundException(documentId);
}
6.3 Define the workflow
See §2.1.

6.4 Register with the host
csharp
builder.Services
    .AddWorkflowEngine()
    .AddBuiltInFlowObjects()
    .AddWorkflowDefinitionsFromAssembly(typeof(Program).Assembly)   // scans IWorkflowDefinition<T>
    .AddDocumentStateProvider<PurchaseOrderStateProvider, PurchaseOrder>()
    .ValidateDefinitionsOnStartup();                                 // fail-fast
Invalid graphs, missing handlers, or missing providers stop the host at startup — not in production.

6.5 Start an instance (via API)
text
POST /api/processes
{ "definitionId": "purchase-order", "version": 3, "document": { "documentId": "PO-123", "amount": 15000, "department": "ops" } }
→ 202 Accepted, { "instanceId": "..." }
6.6 Complete a task (via API)
text
POST /api/processes/{instanceId}/tasks/{taskId}/complete
The service resumes the instance, steps the token, and persists. If the next node is an UpdateDocument, the provider is invoked; otherwise the persisted snapshot continues.

7. Failure model (what to expect)
Failure	Raised by	Outcome
Invalid definition graph	Build() / startup validator	WorkflowDefinitionException; host fails fast
Definition drift on resume	Runtime	DefinitionDriftException; instance faulted with diagnostic
Document type mismatch on load	IExecutionContext.GetDocument<T>	DocumentTypeMismatchException
Document not found at fetch	IDocumentStateProvider<T>	DocumentNotFoundException → OnNotFound branch, or faulted if unconfigured
Any other fetch exception	IDocumentStateProvider<T>	Transient; standard host retry policy
8. Extending the library
Custom node types — extend the Fluent DSL with an extension method on IStartNodeBuilder<T>, add an IFlowNodeHandler implementation, register it. The graph validator will enforce startup correctness.

Cross-cutting behavior — implement IBehaviorExtension; it wraps every node execution.

Data-driven conditions — implement IExpressionEvaluator if you need conditions as data (delegates from the fluent DSL don't need it).

Alternate definition sources — implement IWorkflowDefinitionProvider (default is the in-assembly registry).

All extension points are declared in Bpmn.Workflow.Abstractions and resolved from DI.

9. Testing strategy (mirror this if you add tests)
DSL / graph / hash: pure unit tests in Bpmn.Workflow.Tests.

Engine integration: deterministic tests with an in-memory store; assert resume behavior and "provider not called on resume".

Persistence: Testcontainers against the real DB.

API: WebApplicationFactory with a fake IMessageSession. No real broker in CI.

Architecture: NetArchTest rules for dependency and namespace discipline.

10. Glossary
Term	Meaning
Definition	Immutable, hashed graph produced by the fluent builder.
Instance	A running execution of a definition for one document.
Token	An execution cursor inside an instance. One instance may hold several.
Document	Developer-typed POCO implementing IWorkflowDocument; the process's data.
Snapshot	The persisted copy of the document carried on the instance between refreshes.
Provider	IDocumentStateProvider<T> — developer's authoritative reader.
Handler	IFlowNodeHandler — node behavior, resolved by descriptor.
Descriptor	HandlerDescriptor — type-or-key reference to a handler/provider; never an instance.
Wait state	A node that returns ExecutionStatus.Waiting; instance is persisted and resumed later.
Condition id	Stable string that participates in the definition hash instead of the lambda body.
Drift	Instance pinned to a definition whose hash has changed under it; causes DefinitionDriftException.
11. Non-goals (out of scope, by design)
XML BPMN parsing/upload. Definitions are code. (An optional codegen tool may translate BPMN XML to fluent definitions, but it is not part of the runtime.)

Engine-managed document concurrency. The developer owns it; the engine pulls snapshots.

Runtime definition mutation. Definitions are part of the deploy artifact.

API-side workflow execution. The API is a façade over NServiceBus.

Ad-hoc document patching via API. External systems mutate the document in their own store; the workflow re-reads on UpdateDocument.

12. Contributing (LLM-facing notes)
When modifying this codebase, prefer the smallest change that respects §4 Invariants.

If you add a public type, it goes in Bpmn.Workflow.Abstractions only if the engine needs it by contract (invocation or type constraint). Otherwise it belongs in Bpmn.Workflow.Engine (Bpmn.Workflow.Core.*).

If you add an exception, place it in the namespace of the contract that raises it, or in Bpmn.Workflow if genuinely shared.

If you add a node, extend the DSL and provide an IFlowNodeHandler; the graph model gets a new FlowNodeType and an I*Task interface in FlowObjects.

If you touch the definition hash, be aware that any change breaks every running instance unless the version is bumped. Add a test that pins the hash for a fixture graph.

If you touch UpdateDocument, re-read §4.5–§4.7: the "no refresh except at an explicit node" rule and "wait persistence includes the snapshot" rule are load-bearing.

If you touch the API, do not introduce references to Persistence or Engine.

If you touch namespaces, remember the assembly↔namespace map in §3.7 and the architecture test.

This README is meant to be used as context when prompting an LLM to extend or modify the solution. Keep §3 (Design concepts) and §4 (Invariants) in the prompt whenever you request architectural changes.
