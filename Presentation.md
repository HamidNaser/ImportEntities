# ImportEntities — Project Presentation

## Project Name

ImportEntities — Reusable Multi-Source Entity Import Platform

## Description

ImportEntities is a .NET 8 import platform designed to ingest entities from multiple source types through a single reusable pipeline. It combines source adapters, generic orchestration, business-rule processing, and repository persistence behind clean interfaces so new import scenarios can be added without rewriting the entire flow.

The implementation focuses on extensibility and operational discipline: source-agnostic ingestion, layered architecture, config-driven integrations, structured logging, archive support for replay/debugging, and automated verification through unit and integration tests.

## Skills Demonstrated

- Layered architecture with clear boundaries between clients, domain services, repositories, and shared primitives
- Generic pipeline design using reusable orchestration and source abstractions
- Dependency injection and composition for client-specific import flows
- Integration with external systems through configuration-driven API and storage adapters
- Observability patterns using structured logging, audit context, and blob archival
- Test strategy that validates both domain batching behavior and end-to-end repository/import flows

## Architecture

```mermaid
graph LR
    SRC_HTTP["ApiHelperEntitiesSource<br/>(REST / JSON API)"] --> A
    SRC_CSV["CsvEntitiesSource<br/>(CSV File)"] --> A
    SRC_KAFKA["KafkaEntitiesSource<br/>(Kafka Topic)"] --> A
    SRC_SQS["SqsEntitiesSource<br/>(AWS SQS)"] --> A
    SRC_NEW["YourNewSource<br/>(Anything)"] --> A

    A["IEntitiesSource&lt;T&gt;<br/>(Source Contract)"] --> B["EntitiesImporter&lt;T1,T2&gt;<br/>(Orchestration)"]
    B --> C["IEntitiesService&lt;T&gt;<br/>(Business Rules)"]
    C --> D["IEntitiesRepository<br/>(Repository)"]
    D --> E[("Database / API")]

    H["Error Handling"] -.->|Fallback| B
    H -.->|Logging| I["Audit Trail"]

    style SRC_HTTP fill:#c8e6c9
    style SRC_CSV fill:#c8e6c9
    style SRC_KAFKA fill:#c8e6c9
    style SRC_SQS fill:#c8e6c9
    style SRC_NEW fill:#fff9c4
    style A fill:#e1f5ff
    style B fill:#fff3e0
    style C fill:#f3e5f5
    style D fill:#e8f5e9
    style E fill:#ffebee
    style I fill:#f0f4c3
```

## Validation

```mermaid
graph TD
    UNIT["Unit Tests<br/>(2 Tests)"]
    UNIT -->|"Validates"| DOMAIN["Domain Service<br/>Batching Logic"]

    INT["Integration Tests<br/>(2 Tests)"]

    INT_BATCH["Batch Scenario Test"]
    INT_BATCH -->|"Uses"| FAKE_HTTP["Fake HTTP Handler<br/>(Records Calls)"]
    FAKE_HTTP -->|"Mocks"| IMPORT_CLIENT["Import Client"]
    IMPORT_CLIENT -->|"Calls"| IMPORTER_SERVICE["EntitiesImporter<br/>Service"]
    IMPORTER_SERVICE -->|"Validates"| FAKE_REPO["Fake Repository<br/>(In-Memory)"]

    INT_REPO["Repository Flow Test"]
    INT_REPO -->|"Validates"| REAL_REPO["EntitiesRepository<br/>Update Behavior"]
    REAL_REPO -->|"Verifies"| DATA["Data Persistence<br/>Correctness"]

    INT --> INT_BATCH
    INT --> INT_REPO

    UNIT -->|"Pass"| RESULT1["✓ 2/2 Unit Green"]
    INT -->|"Pass"| RESULT2["✓ 2/2 Integration Green"]

    RESULT1 --> FINAL["4/4 Tests Green<br/>Solution Valid"]
    RESULT2 --> FINAL

    FINAL -->|"Confirms"| CHECKS["✓ Layer Communication<br/>✓ HTTP Contracts<br/>✓ Batching Behavior"]

    style UNIT fill:#f3e5f5
    style INT fill:#fff3e0
    style INT_BATCH fill:#e1f5ff
    style INT_REPO fill:#e8f5e9
    style FAKE_HTTP fill:#fff9c4
    style FAKE_REPO fill:#fff9c4
    style REAL_REPO fill:#fce4ec
    style RESULT1 fill:#c8e6c9
    style RESULT2 fill:#c8e6c9
    style FINAL fill:#81c784
    style CHECKS fill:#c8e6c9
```

Current validation:
- Unit tests: `2/2` pass
- Integration tests: `2/2` pass
- Total: `4/4` pass
