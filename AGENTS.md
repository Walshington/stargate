# Agent context: Stargate / ACTS

This document gives AI agents and contributors context on how the project is structured and how the API works, so changes stay consistent and well-informed.

---

## Project overview

- **Stargate** is an **Astronaut Career Tracking System (ACTS)**. It keeps a record of people who have served as astronauts and their duties (rank, title, start/end dates).
- **Repo layout:**
  - **`stargate/api/`** — .NET 8 REST API (current focus). Run and modify this when working on the backend.
  - **Angular frontend** — Not present yet; planned for the future. Do not assume a `frontend/` or `angular/` app exists.

---

## API architecture

The API uses a **MediatR-based CQRS-style** split: controllers handle HTTP only; business logic lives in the Business layer and is invoked by **request type**, not by direct handler references.

### Flow

1. **Controller** receives an HTTP request, builds a single **request** object (command or query), and calls `_mediator.Send(request)`.
2. **MediatR** finds the handler for that request **type** (e.g. `GetPersonByName` → `GetPersonByNameHandler`) and runs it.
3. **Handler** (in Business layer) uses `StargateContext` (and sometimes Dapper) to read/write data and returns a result.
4. **Controller** maps the result to HTTP via `this.GetResponse(result)` (status code and body come from the result’s `BaseResponse`-style properties).

Controllers do **not** reference handlers or `StargateContext` directly. Adding a new feature means adding a new request + handler (and optionally pre-processors); controllers stay thin.

### Directory layout (`stargate/api/`)

| Area | Purpose |
|------|--------|
| **Controllers/** | HTTP only: routing, `Send(request)`, `GetResponse(result)`. No business or data logic. |
| **Business/Data** | EF Core `StargateContext`, entities (`Person`, `AstronautDetail`, `AstronautDuty`), migrations. |
| **Business/Queries** | Read operations: request + handler + result type (e.g. `GetPersonByName`, `GetPeople`, `GetAstronautDutiesByName`). |
| **Business/Commands** | Write operations: request + optional pre-processors + handler + result (e.g. `CreatePerson`, `CreateAstronautDuty`). |
| **Business/Dtos** | Data transfer shapes for API responses (e.g. `PersonAstronaut`), not raw entities. |

---

## API surface

- **Base URL:** When running locally, the API listens at **http://localhost:5204** (or the port shown in the console). **Swagger:** `http://localhost:5204/swagger`.
- **Response shape:** All responses go through `BaseResponse`: `Success`, `Message`, `ResponseCode`. Many handlers extend it with extra properties (e.g. `Person`, `Id`).

### Person (`/Person`)

| Method | Route | Request / behavior |
|--------|--------|---------------------|
| GET | `/Person` | Returns all people. Request: `GetPeople`. |
| GET | `/Person/{name}` | Returns one person by name (with astronaut detail if any). Request: `GetPersonByName`. |
| POST | `/Person` | Body: plain string (name). Creates person. Request: `CreatePerson`. |

### AstronautDuty (`/AstronautDuty`)

| Method | Route | Request / behavior |
|--------|--------|---------------------|
| GET | `/AstronautDuty/{name}` | Returns astronaut duties for person by name. Currently implemented by sending `GetPersonByName` (not a dedicated duties query). |
| POST | `/AstronautDuty` | Body: JSON for `CreateAstronautDuty` (Name, Rank, DutyTitle, DutyStartDate). Adds an astronaut duty for that person. |

---

## Domain rules (from README)

When changing behavior or adding features, enforce these:

1. A **Person** is uniquely identified by **Name**.
2. A person with no astronaut assignment has no Astronaut records.
3. A person has at most **one current** Astronaut Duty (Title, Start Date, Rank) at a time.
4. **Current duty** has no Duty End Date.
5. When a **new** Astronaut Duty is received for a person, the **previous** duty’s end date is set to the day before the new duty’s start date.
6. A person is **Retired** when Duty Title is `'RETIRED'`.
7. **Career End Date** is one day before the Retired duty’s start date.

---

## Tech stack (API)

- **.NET 8**, ASP.NET Core
- **MediatR** — request/handler dispatch; one request type → one handler
- **EF Core** — `StargateContext`, migrations, SQLite (connection string: `StarbaseApiDatabase`)
- **Dapper** — used in some queries/commands via `StargateContext.Connection`
- **Swagger** — enabled in Development

---

## Running the API

From `stargate/api/`:

```powershell
dotnet restore
dotnet ef database update   # create/update SQLite starbase.db
dotnet run
```

---

## Guidance for agents

- **Follow existing patterns:** New use cases = new request + handler (and result type). Controllers only `Send` and `GetResponse`. Don’t put business or data access in controllers.
- **Where to add code:** New queries → `Business/Queries/`. New commands → `Business/Commands/`. New response shapes → extend `BaseResponse` or add DTOs under `Business/Dtos/`. New endpoints → add actions to the appropriate controller and a corresponding request/handler.
- **Testing:** Controllers can be tested by mocking `IMediator`. Handlers can be tested in isolation with a real or fake `StargateContext`. The MediatR split is intended to make both easy.
- **Frontend:** An Angular frontend is planned but not in the repo yet. Do not assume or create frontend paths unless the user asks for them.
