# bpmn.workflow
Bpmn.Workflow — README
Bpmn.Workflow — README
Purpose. This document is the canonical primer for the solution — both for human developers and for LLMs using it as context. It explains what the solution is, why it's shaped this way, and which invariants must not be broken. If you are an LLM contributing code, read Design Concepts and Invariants first — they encode the load-bearing decisions.

Table of Contents
What this is

Key features

Design concepts

Invariants

Repository layout

Using the library

Failure model

Extending the library

Testing strategy

Glossary

Non-goals

Contributing

1. What this is
A developer-first, fluent-API BPMN workflow library for .NET 8. Workflows are defined in C# code (not uploaded as XML), operate on a strongly-typed developer document, and execute on a transport-agnostic token-based engine with durable persistence across wait states.

The solution is composed of two core libraries + two transport adapters + supporting contracts, deliberately separated so that engine logic, API logic, message contracts, and broker SDKs do not leak into each other.

Assembly	Role	Broker refs
Bpmn.Workflow.Abstractions	Public contracts only. No dependencies. The surface developers code against.	none
Bpmn.Workflow.Engine	Fluent DSL implementation, built-in flow-object handlers, graph validation, definition hashing. Root ns: Bpmn.Workflow.Core.	none
Bpmn.Workflow.Persistence	Persistence contracts: entity POCOs, store interfaces, completion outcomes.	none
Bpmn.Workflow.Persistence.EfCore	EF Core implementation: DbContext, entity mappings, migrations, store impls.	EF Core
Bpmn.Workflow.Messages	Transport-agnostic message and integration-event POCOs shared by API and runtime.	none
Bpmn.Workflow.Runtime	Transport-agnostic runtime core: engine loop, definition scanning, dispatcher seam. Library, not a host.	none
Bpmn.Workflow.Runtime.NServiceBus	NServiceBus transport adapter for the runtime (message handlers, endpoint lifecycle).	NServiceBus
Bpmn.Workflow.Api	Transport-agnostic ASP.NET Core core: controllers, read models, command-publisher seam. Library, not a host.	none
Bpmn.Workflow.Api.NServiceBus	NServiceBus transport adapter for the API (send-only endpoint, publish/execute seam).	NServiceBus
Call graph
text
 Client ──HTTP──▶ Bpmn.Workflow.Api (+ .NServiceBus adapter)
                       │
                       │  ICommandPublisher / ICommandExecutor   (seam)
                       ▼
                 ── broker (NServiceBus today) ──
                       ▲
                       │  IWorkflowMessageDispatcher             (seam)
                       │
                 Bpmn.Workflow.Runtime (+ .NServiceBus adapter)
                       │
              ┌────────┴─────────┐
              ▼                  ▼
      Bpmn.Workflow.Engine   Bpmn.Workflow.Persistence (+ .EfCore)
Two structural rules:

The API is a façade. It publishes commands and reads projections. It never executes workflow logic and never touches persistence through the runtime.

The runtime core is transport-agnostic. Broker work lives in adapter packages. Adding Rebus or MassTransit later is a new adapter, not a change to the core.

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
                    .Then().UserTask("manager-approval", t => t
                        .Assignees(ctx => new[]
                        {
                            $"user:{ctx.Document.OwnerId}",
                            $"user:{ctx.Document.EscalationOwnerId}"
                        }))
                    .UpdateDocument("refresh", u => u.Provider<PurchaseOrderStateProvider>())
                        .OnRefreshed(n => n.End())
                        .OnNotFound(n => n.End())
                .Branch("low", x => x.Condition("low", ctx => ctx.Document.Amount <= 10_000m))
                    .Then().End()
        .Build();
}
Definitions are compiled artifacts that ship with the application. There is no upload endpoint.

The DSL produces an immutable IProcessDefinition via Build().

Graph validation and a deterministic structural hash are computed at build/startup. Invalid graphs fail fast.

Definition versioning is explicit (Version(int)). Instances pin (Id, Version, Hash); drift raises DefinitionDriftException.

2.2 Typed workflow documents
csharp
public sealed class PurchaseOrder : IWorkflowDocument
{
    public string DocumentId { get; init; } = default!;
    public decimal Amount { get; init; }
    public string OwnerId { get; init; } = default!;
    public string EscalationOwnerId { get; init; } = default!;
    public string Department { get; init; } = default!;
}
The document is the single source of truth for process data. It is persisted on the instance (DocumentJson).

Gateway conditions, assignee expressions, message correlation keys receive IExecutionContext<TDocument> and read ctx.Document.X with compile-time safety.

No revision field. The engine does not manage document concurrency. Developers who need optimistic concurrency model it themselves and enforce it in their provider.

2.3 Pull-based UpdateDocument with snapshot caching
The document's authoritative state is owned outside the workflow. When a workflow needs a fresh copy, it executes an UpdateDocument node:

Engine resolves the developer's IDocumentStateProvider<TDocument> from DI.

Engine calls GetDocumentAsync(documentId, ct).

On success, the fetched document replaces the in-flight snapshot; every subsequent activity sees the new state.

On DocumentNotFoundException, the token routes to the configured OnNotFound branch (or faults if none).

Any other exception is treated as transient; the host retries.

Snapshot caching is the core performance property. At every wait state the instance is persisted with its current document snapshot. Resuming from a wait does not call the provider. The expensive retrieval happens only at explicit UpdateDocument nodes.

This is why the abstraction is IDocumentStateProvider<T> (pull) and not an "updater" (push): the workflow decides when to refresh, not the outside world.

2.4 Multi-assignee user tasks (any-of completion)
A user task carries a set of user principals (string[]) resolved from the document. Any principal may complete the task; the first successful completion wins.

The set is snapshotted on entry and is the authority for completion for the lifetime of the wait.

An empty set at entry faults the token with user-task-no-assignees.

Groups and roles are resolved to concrete users by the developer's expression — the engine is identity-agnostic.

No FormKey. UI metadata does not belong in the graph.

Completion outcomes are structured: not-an-assignee (403), task-already-completed (409), not-found (404). The engine enforces first-wins via instance-level optimistic concurrency — no per-task lock.

2.5 Transport-agnostic core + adapters
The runtime and API cores are libraries, not executables. The developer's console/worker/web app composes them.

Broker work lives in adapter packages (Bpmn.Workflow.Runtime.NServiceBus, Bpmn.Workflow.Api.NServiceBus). Adding Rebus or MassTransit later is a new adapter, not a core change.

The cores compile with zero broker SDK references (enforced by architecture tests).

2.6 Token-based execution
Instances hold one or more tokens (CurrentNodeId, Status, WaitReason, CorrelationKey). This is the only model that correctly handles parallel gateways, inclusive gateways, and joins. When a token reaches a wait state, the instance is persisted and the transaction completes.

2.7 Feature-driven namespaces
Namespaces mirror features, not layers. Exceptions live next to (or in the root of) the contracts that produce them.

Bpmn.Workflow — shared root (WorkflowDefinitionException).

Bpmn.Workflow.Documents — typed-document feature.

Bpmn.Workflow.Definitions — definition model; .Fluent sub-namespace for the DSL.

Bpmn.Workflow.FlowObjects — frozen graph model.

Bpmn.Workflow.Execution — engine execution contracts and execution exceptions.

Bpmn.Workflow.Extensibility — pluggable hooks.

Bpmn.Workflow.Persistence.ProcessInstances, .Tokens, .Activities, .Variables, .Definitions, .Documents, .Stores — feature folders in persistence.

Bpmn.Workflow.Runtime.Hosting, .Engine, .Definitions, .Configuration — runtime features.

Bpmn.Workflow.Api.Messaging, .ReadModels, .Controllers, .Dtos, .Security, .Configuration — API features.

3. Design concepts
3.1 Contract / implementation split
Bpmn.Workflow.Abstractions is the only assembly a consumer must reference. It has zero dependencies beyond the BCL, zero implementation, and zero types from other Bpmn assemblies. If you find yourself wanting to add "a little logic" to it, stop — add it to Bpmn.Workflow.Engine and expose it via a contract.

3.2 Definitions are immutable and hashable
IProcessDefinition is frozen after Build(). The definition hash is computed over:

node ids and node types,

edges (incoming/outgoing),

handler/provider descriptors,

developer-supplied ConditionId strings (not the lambda bodies).

Refactoring a lambda body does not change the hash; changing a ConditionId does. Any structural edit must bump Version(int). Instances pin (Id, Version, Hash) and raise DefinitionDriftException on mismatch.

3.3 Documents are snapshots
An instance's document is a point-in-time snapshot. The authoritative state lives wherever the developer's IDocumentStateProvider<T> reads it from. Between UpdateDocument nodes the workflow sees the snapshot from the last fetch. This is intended — freshness is opt-in, and authors place refresh nodes where they need it.

3.4 Handlers are descriptors, not instances
HandlerDescriptor identifies a handler by type or key. It never holds an instance. This makes definitions freezable and hashable and keeps DI resolution at runtime.

3.5 NodeExecutionResult is the single engine contract
Every node returns this shape:

Status: Continue | Waiting | Completed | Faulted,

NextNodeIds,

optional VariableUpdates, WaitReason, CorrelationKey, FaultReason.

If you find yourself wanting a special-case return type for a node, resist it. Extend NodeExecutionResult if genuinely general; otherwise model custom behavior within these four statuses.

3.6 The API is a façade
The API:

publishes commands via ICommandPublisher / ICommandExecutor (transport-neutral seams),

reads projections (read models) for status,

lists definitions and returns a graph projection (read-only),

returns the persisted document snapshot (read-only; explicitly not the live source).

The API must not:

execute workflow logic,

reference Bpmn.Workflow.Runtime, Bpmn.Workflow.Engine, or Bpmn.Workflow.Persistence.EfCore (beyond read-model reads),

mutate definitions,

mutate documents,

reference any broker SDK.

3.7 Runtime and API are libraries, not hosts
Both Bpmn.Workflow.Runtime and Bpmn.Workflow.Api are libraries. The developer's console/worker/web app owns the entry point:

Runtime host: builder.Services.AddWorkflowRuntime()...; await builder.Build().RunAsync();

API host: builder.Services.AddWorkflowApi(); app.MapWorkflowApi(); app.Run();

Provider packages (transport, EF Core provider) are added by the host, not by the cores.

3.8 Adapters implement seams
Two seams, one per side:

Runtime side: IWorkflowMessageDispatcher (transport-neutral "process this message" entry point) and IWorkflowRuntimeHost (transport-neutral lifecycle).

API side: ICommandPublisher (publish) and ICommandExecutor (request/response, for synchronous outcomes such as complete-task).

Adapters (*.NServiceBus) translate broker messages into seam calls and map results back to broker semantics. They contain no engine logic and no API business logic.

3.9 Namespace-to-assembly discipline
Assembly	Root namespace
Bpmn.Workflow.Abstractions	Bpmn.Workflow
Bpmn.Workflow.Engine	Bpmn.Workflow.Core
Bpmn.Workflow.Persistence	Bpmn.Workflow.Persistence
Bpmn.Workflow.Persistence.EfCore	Bpmn.Workflow.Persistence.EfCore
Bpmn.Workflow.Messages	Bpmn.Workflow.Messages
Bpmn.Workflow.Runtime	Bpmn.Workflow.Runtime
Bpmn.Workflow.Runtime.NServiceBus	Bpmn.Workflow.Runtime.NServiceBus
Bpmn.Workflow.Api	Bpmn.Workflow.Api
Bpmn.Workflow.Api.NServiceBus	Bpmn.Workflow.Api.NServiceBus
No public contract in Abstractions may reference Bpmn.Workflow.Core.*. The cores must not reference broker SDKs. All enforced by architecture tests.

4. Invariants
These are the non-negotiables. Most regressions in a workflow engine come from violating one of them.

Abstractions has no dependencies (BCL only) and exposes no implementation types.

Definitions are immutable after Build(). No mutation of IProcessDefinition or its nodes.

Definition hash is deterministic. Derived from structural data + explicit ids, never from delegate bodies, reflection order, or runtime state.

Instances pin to (Id, Version, Hash). Resume on mismatch raises DefinitionDriftException. Never silently re-map to a newer definition.

The engine swaps the document only at UpdateDocument nodes. There is no implicit refresh — not on resume, not on wait-state transitions, not on retries.

Wait-state persistence includes the current document snapshot. A crash or restart must resume with the same snapshot the instance had at the moment of waiting.

IDocumentStateProvider<T> is called only when a token executes an UpdateDocument node. A test asserting "provider not called on resume" must pass.

The API never references the runtime, the engine, or EF Core persistence. All execution goes through ICommandPublisher / ICommandExecutor.

NodeExecutionResult is the only execution return shape. No side-channel result types.

Externals are authoritative. The engine never authoritatively mutates the document. It replaces its snapshot with what the provider returned.

Namespace discipline holds. Bpmn.Workflow.Abstractions owns the Bpmn.Workflow root; implementation assemblies never shadow that tree with their own public types.

Failure classification is explicit. DocumentNotFoundException = permanent → route to OnNotFound. Any other exception = transient → host retry.

User-task assignee sets are snapshotted on entry and are the authority for completion for the lifetime of the wait. Completion authorization never re-evaluates the expression against the current document. Assignee sets are arrays of concrete user principals; groups and roles are resolved by the developer's expression, not by the engine. Any-of completion is enforced by instance-level optimistic concurrency — no per-task lock.

Bpmn.Workflow.Runtime and Bpmn.Workflow.Api are libraries, not hosts. They expose composition APIs (AddWorkflowRuntime, AddWorkflowApi, RunAsync) and must not contain Program.cs, OutputType=Exe, or provider-specific package references (transport, EF Core provider).

The runtime depends on Bpmn.Workflow.Persistence (contracts), not on Bpmn.Workflow.Persistence.EfCore. EF Core wiring is composed by the host; the runtime stays persistence-provider-agnostic at the composition boundary.

The core runtime is transport-agnostic. Bpmn.Workflow.Runtime must not reference any broker SDK. All transport work lives in adapter assemblies.

Adapters translate, they do not execute. A runtime transport adapter deserializes the broker message, invokes IWorkflowMessageDispatcher, and maps the result back. It must not contain engine logic, token routing, or persistence calls beyond the dispatcher/outbox.

Message contracts live in Bpmn.Workflow.Messages. POCOs with no broker dependencies. Broker-specific attributes do not go on these contracts; adapters attach metadata via their own configuration.

The core API is transport-agnostic. Bpmn.Workflow.Api must not reference any broker SDK.

The API communicates with the runtime only via messages. Bpmn.Workflow.Api must not reference Bpmn.Workflow.Runtime, Bpmn.Workflow.Engine, or Bpmn.Workflow.Runtime.NServiceBus.

API adapters translate, they do not orchestrate. A transport adapter implements ICommandPublisher / ICommandExecutor over its broker SDK. It must not contain API business logic, read-model queries, or controller behavior.

5. Repository layout
text
Bpmn.Workflow.sln
├── src/
│   ├── Bpmn.Workflow.Abstractions/                 # contracts; BCL only
│   ├── Bpmn.Workflow.Engine/                       # fluent DSL + built-in handlers; ns Bpmn.Workflow.Core
│   ├── Bpmn.Workflow.Persistence/                  # entities + store interfaces; no EF
│   ├── Bpmn.Workflow.Persistence.EfCore/           # DbContext, configs, migrations, EF stores
│   ├── Bpmn.Workflow.Messages/                     # transport-agnostic POCOs; BCL only
│   ├── Bpmn.Workflow.Runtime/                      # transport-agnostic runtime core; LIBRARY
│   ├── Bpmn.Workflow.Runtime.NServiceBus/          # NServiceBus runtime adapter
│   ├── Bpmn.Workflow.Api/                          # transport-agnostic ASP.NET Core core; LIBRARY
│   └── Bpmn.Workflow.Api.NServiceBus/              # NServiceBus API adapter
│
├── tests/
│   ├── Bpmn.Workflow.Tests/                        # DSL, validation, hash, handlers
│   ├── Bpmn.Workflow.Persistence.Tests/            # entities / completion outcomes (no DB)
│   ├── Bpmn.Workflow.Persistence.EfCore.Tests/     # EF stores, migrations (Testcontainers)
│   ├── Bpmn.Workflow.Runtime.Tests/                # engine + dispatcher (no broker)
│   ├── Bpmn.Workflow.Runtime.NServiceBus.Tests/    # adapter translation
│   ├── Bpmn.Workflow.Api.Tests/                    # controllers + read models, fake publisher/executor
│   └── Bpmn.Workflow.Api.NServiceBus.Tests/        # adapter translation
│
├── samples/
│   ├── Bpmn.Sample.Definitions/                    # PurchaseOrder + provider + workflow + handlers
│   ├── Bpmn.Sample.Host/                           # console app composing runtime + NSB adapter
│   └── Bpmn.Sample.Api/                            # web app composing API + NSB adapter
│
├── build/
│   ├── Directory.Build.props
│   ├── Directory.Packages.props
│   ├── version.json
│   └── ci/
│
├── docs/
│   ├── architecture.md
│   ├── fluent-dsl-guide.md
│   ├── typed-documents.md
│   ├── extension-guide.md
│   └── supported-elements-matrix.md
│
└── Bpmn.Workflow.sln
Dependency rules (enforced by NetArchTest)
Project	May reference
Bpmn.Workflow.Abstractions	BCL only
Bpmn.Workflow.Engine	Abstractions
Bpmn.Workflow.Persistence	Abstractions
Bpmn.Workflow.Persistence.EfCore	Abstractions, Persistence, EF Core packages
Bpmn.Workflow.Messages	BCL only
Bpmn.Workflow.Runtime	Abstractions, Engine, Persistence, Messages, Microsoft.Extensions.*
Bpmn.Workflow.Runtime.NServiceBus	Abstractions, Runtime, Messages, NServiceBus
Bpmn.Workflow.Api	Abstractions, Messages, Persistence, Persistence.EfCore, ASP.NET Core
Bpmn.Workflow.Api.NServiceBus	Api, Messages, NServiceBus
Additional architecture rules:

No NServiceBus.* reference in Bpmn.Workflow.Runtime or Bpmn.Workflow.Api.

Bpmn.Workflow.Api does not reference Runtime, Engine, or Runtime.NServiceBus.

No Microsoft.EntityFrameworkCore.* reference in Bpmn.Workflow.Persistence (only in .EfCore).

No Bpmn.Workflow.Core.* type on any public API surface of Abstractions.

PublicApiAnalyzers tracks the public API of every package across versions.

6. Using the library
6.1 Define a document
csharp
public sealed class PurchaseOrder : IWorkflowDocument
{
    public string DocumentId { get; init; } = default!;
    public decimal Amount { get; init; }
    public string OwnerId { get; init; } = default!;
    public string EscalationOwnerId { get; init; } = default!;
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
See §2.1. Assignees are resolved to concrete users:

csharp
.UserTask("manager-approval", t => t.Assignees(ctx => new[]
{
    $"user:{ctx.Document.OwnerId}",
    $"user:{ctx.Document.EscalationOwnerId}"
}))
6.4 Host the runtime (developer's console/worker app)
csharp
using Bpmn.Workflow.Runtime.Configuration;
using Bpmn.Workflow.Runtime.NServiceBus.Configuration;
using Bpmn.Workflow.Persistence.EfCore.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// 1. Transport choice: NServiceBus today; Rebus/MassTransit later.
builder.UseNServiceBusWorkflow(o =>
{
    o.EndpointName = "PurchaseOrderWorkflow";
    o.Transport = NServiceBusTransport.RabbitMQ;
    o.ConnectionString = builder.Configuration.GetConnectionString("Rabbit")!;
    o.RecoverabilityDelayedRetries = 5;
});

// 2. Core runtime + developer's definitions/providers/handlers.
builder.Services
    .AddWorkflowRuntime()
    .AddBuiltInFlowObjects()
    .AddWorkflowDefinitionsFromAssembly(typeof(Program).Assembly)
    .AddDocumentStateProvider<PurchaseOrderStateProvider, PurchaseOrder>()
    .ValidateDefinitionsOnStartup();

// 3. Persistence (developer's choice of provider).
builder.Services.AddEfCoreWorkflowPersistence(
    options => { },
    ef => ef.UseSqlServer(builder.Configuration.GetConnectionString("Workflow")));

await builder.Build().RunAsync();
Invalid graphs, missing handlers, or missing providers stop the host at startup — not in production.

6.5 Host the API (developer's ASP.NET Core app)
csharp
using Bpmn.Workflow.Api.Configuration;
using Bpmn.Workflow.Api.NServiceBus.Configuration;
using Bpmn.Workflow.Persistence.EfCore.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 1. Transport choice: NServiceBus today; Rebus/MassTransit later.
builder.Services.AddNServiceBusApiAdapter(o =>
{
    o.EndpointName = "WorkflowApi";
    o.Transport = NServiceBusTransport.RabbitMQ;
    o.ConnectionString = builder.Configuration.GetConnectionString("Rabbit")!;
});

// 2. Core API (controllers, read models, DTOs) — transport-agnostic.
builder.Services.AddWorkflowApi();

// 3. Read-model persistence.
builder.Services.AddEfCoreWorkflowPersistence(
    options => { },
    ef => ef.UseSqlServer(builder.Configuration.GetConnectionString("Workflow")));

var app = builder.Build();
app.MapWorkflowApi();
app.Run();
A future Rebus host swaps only step 1: builder.Services.AddRebusApiAdapter(...).

6.6 API endpoints
Method	Path	Description
POST	/api/processes	Start an instance. Body: { definitionId, version, document }. Returns 202 Accepted + instanceId.
GET	/api/processes/{id}	Read-model: status, tokens, history, document snapshot metadata.
POST	/api/processes/{id}/tasks/{taskId}/complete	Complete a user task. First caller in assignees wins. Returns 200 / 403 not-an-assignee / 409 task-already-completed / 404.
POST	/api/processes/{id}/messages	Deliver an external message to a waiting MessageCatchEvent.
POST	/api/processes/{id}/cancel	Cancel a running or waiting instance.
GET	/api/definitions	Read-only list of compiled definitions (metadata).
GET	/api/definitions/{id}/graph	Read-only graph projection.
GET	/api/processes/{id}/document	Read-only document snapshot + provenance (Initial / Refreshed + timestamp).
The document endpoint returns the persisted snapshot, not the live source. It is explicitly labelled as such in the response metadata.

7. Failure model
Failure	Raised by	Outcome
Invalid definition graph	Build() / startup validator	WorkflowDefinitionException; host fails fast
Definition drift on resume	DefinitionDriftChecker	DefinitionDriftException; instance faulted with diagnostic
Document type mismatch on load	IExecutionContext.GetDocument<T>	DocumentTypeMismatchException
Document not found at fetch	IDocumentStateProvider<T>	DocumentNotFoundException → OnNotFound branch, or faulted if unconfigured
Any other fetch exception	IDocumentStateProvider<T>	Transient; standard host retry policy
Empty user-task assignee set	UserTaskHandler	NodeExecutionResult.Faulted("user-task-no-assignees")
Non-assignee completion	Runtime command handler	API returns 403 not-an-assignee
Concurrent completion loss	Optimistic concurrency + retry	API returns 409 task-already-completed
Unknown task id	Runtime command handler	API returns 404
8. Extending the library
Custom node types — extend the Fluent DSL with an extension method on IStartNodeBuilder<T>, add an IFlowNodeHandler implementation, register it. The graph validator enforces startup correctness.

Cross-cutting behavior — implement IBehaviorExtension; it wraps every node execution.

Data-driven conditions — implement IExpressionEvaluator if you need conditions as data (delegates from the fluent DSL do not require it).

Alternate definition sources — implement IWorkflowDefinitionProvider (default is the in-assembly registry).

Alternate transport — implement IWorkflowRuntimeTransportAdapter + the dispatcher lifecycle for the runtime, and ICommandPublisher / ICommandExecutor for the API. Add a package Bpmn.Workflow.Runtime.<Broker> and Bpmn.Workflow.Api.<Broker>. Do not modify the cores.

Alternate persistence — implement the store interfaces from Bpmn.Workflow.Persistence in a new package (e.g. Bpmn.Workflow.Persistence.Dapper). The engine depends only on contracts.

Alternate document schemas — implement IDocumentMigrator<T> to upgrade persisted snapshots on load.

All extension points are declared in Bpmn.Workflow.Abstractions (for workflow/engine) or the respective core assembly (for runtime/API seams) and resolved from DI.

9. Testing strategy
Mirror these tiers if you add tests:

DSL / graph / hash: pure unit tests in Bpmn.Workflow.Tests.

Engine integration: deterministic tests with an in-memory store; assert resume behavior and "provider not called on resume".

Persistence contracts: pure unit tests for entities and UserTaskCompletionResult — no DB.

EF Core persistence: Testcontainers against the real DB.

Runtime core: build an in-process host via AddWorkflowRuntime(...) with a fake IWorkflowMessageDispatcher caller. No broker.

Runtime NServiceBus adapter: translation tests with a fake IMessageSession; integration tests with Testcontainers broker if needed.

API core: WebApplicationFactory with fake ICommandPublisher / ICommandExecutor. No broker, no runtime.

API NServiceBus adapter: translation tests with a fake IMessageSession.

Architecture: NetArchTest rules from §5.

The fact that Bpmn.Workflow.Runtime.Tests and Bpmn.Workflow.Api.Tests run without any broker package is the concrete proof that the seams are real.

10. Glossary
Term	Meaning
Definition	Immutable, hashed graph produced by the fluent builder.
Instance	A running execution of a definition for one document.
Token	An execution cursor inside an instance. One instance may hold several.
Document	Developer-typed POCO implementing IWorkflowDocument; the process's data.
Snapshot	The persisted copy of the document carried on the instance between refreshes.
Provider	IDocumentStateProvider<T> — developer's authoritative reader.
Handler	IFlowNodeHandler — node behavior, resolved by descriptor.
Descriptor	HandlerDescriptor — type-or-key reference; never an instance.
Wait state	A node that returns ExecutionStatus.Waiting; instance is persisted and resumed later.
Condition id	Stable string that participates in the definition hash instead of the lambda body.
Drift	Instance pinned to a definition whose hash has changed under it; causes DefinitionDriftException.
Assignee set	Snapshotted array of user principals that may complete a user task.
Any-of completion	First successful completion wins; enforced by instance-level optimistic concurrency.
Seam (runtime)	IWorkflowMessageDispatcher / IWorkflowRuntimeHost — transport-neutral entry points implemented by adapters.
Seam (API)	ICommandPublisher / ICommandExecutor — transport-neutral publish/execute entry points implemented by adapters.
Adapter	Package implementing a seam for a specific broker (*.NServiceBus, future *.Rebus, *.MassTransit).
Host app	The developer's own console/worker/web app that composes the cores + adapters.
11. Non-goals
XML BPMN parsing/upload. Definitions are code. (An optional codegen tool may translate BPMN XML to fluent definitions — off the runtime path.)

Engine-managed document concurrency. The developer owns it; the engine pulls snapshots.

Runtime definition mutation. Definitions are part of the deploy artifact.

API-side workflow execution. The API is a façade over the message bus.

Ad-hoc document patching via API. External systems mutate the document in their own store; the workflow re-reads on UpdateDocument.

Group / role identity in the engine. Assignee expressions resolve concrete users.

UI metadata in the graph (e.g. FormKey). Presentation concerns are resolved by nodeId outside the engine.

Broker abstraction inside the cores. Transport lives in adapters; the cores must not reference broker SDKs.

A single "universal" transport abstraction. Each adapter implements the seam; there is no shared IBroker interface. Adding Rebus means adding a package, not changing the seam.

12. Contributing (LLM-facing notes)
When modifying this codebase, prefer the smallest change that respects §4 Invariants.

Adding a public type: goes in Bpmn.Workflow.Abstractions only if the engine needs it by contract (invocation or type constraint). Otherwise it belongs in Bpmn.Workflow.Engine (Bpmn.Workflow.Core.*) or a feature assembly.

Adding an exception: place it in the namespace of the contract that raises it, or in Bpmn.Workflow if genuinely shared.

Adding a node: extend the DSL, provide an IFlowNodeHandler, add a FlowNodeType, add an I*Task interface in FlowObjects. Update startup validation.

**Touching the definition hash