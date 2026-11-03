# CQRS feature structure

Application use cases are organized by feature and separated into commands and queries.

## Folder convention

Each feature follows this structure:

```text
Features/
  Projects/
    Commands/       # IRequest<T> write requests
    Queries/        # IRequest<T> read requests
    Handlers/       # separate command and query handlers
  WorkItems/
  Sprints/
  Billing/
  Meetings/
  Reminders/
  Users/
  Validators/       # FluentValidation rules for write requests
```

## Rules

1. A **Command** represents an intention to change state (create, update, delete, archive, assign, complete, etc.). Its handler owns the write-side use case and persists changes through `IUnitOfWork`.
2. A **Query** reads data only. Query handlers use `AsNoTracking()` where appropriate and return DTOs, never EF entities.
3. A handler implements one side only: command handlers implement `IRequestHandler<...Command, ...>`; query handlers implement `IRequestHandler<...Query, ...>`.
4. Controllers are transport adapters. They bind HTTP input, call `ISender.Send`, and map the result to HTTP status codes; they must not contain business rules or EF Core access.
5. Input validation belongs in FluentValidation validators. Validators are discovered from the Application assembly and executed by the registered MediatR validation pipeline.
6. The authenticated user and project membership must be checked inside the relevant handler. Never trust user IDs supplied by a request body for audit identity or ownership.
7. Commands must enforce aggregate invariants and cross-entity references before saving. Queries must not call `SaveChanges` or mutate tracked entities.
8. Keep request/response contracts in the Application layer; do not expose Domain entities directly through controllers.

## Refactored feature modules

The command/query split is applied to Projects, WorkItems, Sprints, Billing, Meetings, Reminders and Users. Account login is named as a command handler; Account already has separate command and query folders.

## Adding a new use case

- Add a request record under `Commands` or `Queries`.
- Add a handler under `Handlers` that implements exactly one request contract.
- Add a FluentValidation validator for write commands when input constraints exist.
- Send the request from the controller through MediatR.
- Add tests for validation, successful execution, missing resources, authorization/membership boundaries, and query read-only behavior.
