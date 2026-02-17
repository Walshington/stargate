# Agent context: Stargate / ACTS

This document gives AI agents and contributors context on how the project is structured and how the API works, so changes stay consistent and well-informed.

---

## Project overview

- **Stargate** is an **Astronaut Career Tracking System (ACTS)**. It keeps a record of people who have served as astronauts and their duties (rank, title, start/end dates).
- **Repo layout:**
  - **`stargate/api/`** — .NET 8 REST API. Run and modify this when working on the backend.
  - **`stargate/web/`** — Angular 21 SPA frontend. Provides a UI for searching astronauts and viewing career/duty history.

---

## API architecture

The API uses a **MediatR-based CQRS-style** split: controllers handle HTTP only; business logic lives in the Business layer and is invoked by **request type**, not by direct handler references.

### Flow

1. **Controller** receives an HTTP request, builds a single **request** object (command or query), and calls `_mediator.Send(request)`.
2. **MediatR** finds the handler for that request **type** (e.g. `GetPersonByName` → `GetPersonByNameHandler`) and runs it.
3. **Handler** (in Business layer) uses `StargateContext` (and sometimes Dapper) to read/write data and returns a result.
4. **Controller** maps the result to HTTP via `this.GetResponse(result)` (status code and body come from the result's `BaseResponse`-style properties).

Controllers do **not** reference handlers or `StargateContext` directly. Adding a new feature means adding a new request + handler (and optionally pre-processors); controllers stay thin.

**Note:** The actual execution flow includes:
- **ExceptionHandlingMiddleware** wraps the entire request pipeline
- **LoggingBehavior** automatically intercepts all MediatR requests to log them before/after handler execution
- **Pre-processors** run before the handler for validation and setup
-  Handler exceptions are logged by LoggingBehavior, then re-thrown and caught by ExceptionHandlingMiddleware for HTTP-level logging

### Logging Architecture

All API operations are automatically logged to both console and database:

- **LoggingBehavior** (MediatR pipeline): Captures all command/query executions with request data, response data, duration, and status codes. Logs exceptions with full MediatR context before re-throwing.
- **ExceptionHandlingMiddleware** (HTTP layer): Catches exceptions that bubble up, logs them with HTTP context (method, path, error response), and returns standardized error responses.
- **IApiLoggingService**: Decoupled service that persists logs to the `ApiLog` table using isolated `DbContext` instances to ensure logs survive transaction rollbacks.
- **Dual logging**: Both console (via `ILogger`) and database persistence happen in the same service for complete observability.

### Directory layout (`stargate/api/`)

| Area | Purpose |
|------|--------|
| **Domain/** | Shared API contracts: `BaseResponse`, DTOs (e.g. `PersonAstronaut` under `Domain/Dtos/`), custom exceptions. No dependencies on Controllers or Business. |
| **Controllers/** | HTTP only: routing, `Send(request)`, `GetResponse(result)`. No business or data logic. |
| **Middleware/** | Cross-cutting concerns: `ExceptionHandlingMiddleware` for centralized exception handling and logging. |
| **Business/Data** | EF Core `StargateContext`, entities (`Person`, `AstronautDetail`, `AstronautDuty`, `ApiLog`), migrations. |
| **Business/Queries** | Read operations: request + handler + result type (e.g. `GetPersonByName`, `GetPeople`, `GetAstronautDutiesByName`). |
| **Business/Commands** | Write operations: request + optional pre-processors + handler + result (e.g. `CreatePerson`, `CreateAstronautDuty`). |
| **Business/Behaviors** | MediatR pipeline behaviors: `LoggingBehavior` for automatic request/response logging. |
| **Business/Services** | Cross-cutting services: `IApiLoggingService` for database-backed logging with transaction isolation. |

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
5. When a **new** Astronaut Duty is received for a person, the **previous** duty's end date is set to the day before the new duty's start date.
6. A person is **Retired** when Duty Title is `'RETIRED'`.
7. **Career End Date** is one day before the Retired duty's start date.

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

## Angular frontend (`stargate/web/`)

The frontend is a standalone Angular 21 SPA that lets users search for astronauts and view their career details and duty history.

### Tech stack (frontend)

- **Angular 21** — standalone components, no NgModules
- **Signals** — all state managed via `signal()`, `computed()`, and `input()`/`output()`; no `@Input`/`@Output` decorators
- **RxJS** — HTTP requests via `Observable`, subscriptions cleaned up with `takeUntilDestroyed`
- **HttpClient** — provided via `provideHttpClient()` in `app.config.ts`
- **SCSS** — component-scoped styles plus a global `styles.scss`
- **OnPush change detection** — used on all components for performance
- **Vitest** — unit test runner (configured via `@angular/build:unit-test`)
- **Prettier** — code formatting (`npm run format` / `npm run format:check`)

### Directory layout (`stargate/web/src/app/`)

| Area | Purpose |
|------|---------|
| **app.ts** | Root component. Owns top-level signals: `searchResults`, `isLoading`, `errorMessage`. Handles search events and calls `AstronautApiService`. |
| **app.html** | Root template. Header, search section, loading/error states, results section, and `<router-outlet />` for future routes. |
| **app.config.ts** | Application providers: `provideBrowserGlobalErrorListeners`, `provideRouter`, `provideHttpClient`. |
| **app.routes.ts** | Route definitions (currently empty — no routes defined yet). |
| **components/astronaut-search/** | Search bar component. Signal-based form (`searchName` signal), computed `isSearchDisabled`, emits `astronautSearched` output. Supports Enter key and has accessible ARIA labels. |
| **components/astronaut-duty-list/** | Displays astronaut info and duty history. Signal inputs: `person`, `duties`. Computed: `activeDuty` (no end date) and `inactiveDuties` (has end date). Uses `DatePipe`. |
| **services/astronaut-api.service.ts** | HTTP service. Method: `getAstronautDutiesByName(name)` calls `GET /AstronautDuty/{name}`. Returns `Observable<AstronautDutiesResponse>`. Handles 404, network errors, and other HTTP errors with user-friendly messages. |
| **models/astronaut.model.ts** | Shared TypeScript interfaces: `PersonAstronaut`, `AstronautDuty`, `AstronautDutiesResponse`. |
| **environments/** | `environment.ts` (dev: `apiBaseUrl: http://localhost:5204`) and `environment.prod.ts` (prod: placeholder URL). |

### Data flow

```
User types name → AstronautSearchComponent (emits astronautSearched)
  → App component calls AstronautApiService.getAstronautDutiesByName(name)
    → GET /AstronautDuty/{name} → Backend API
      → Response updates searchResults signal
        → AstronautDutyListComponent receives person + duties as signal inputs
```

### Interfaces (`astronaut.model.ts`)

```typescript
PersonAstronaut         { personId, name, currentRank, currentDutyTitle, careerStartDate, careerEndDate }
AstronautDuty           { id, personId, rank, dutyTitle, dutyStartDate, dutyEndDate }
AstronautDutiesResponse { success, responseCode, person, astronautDuties }
```

### Running the frontend

From `stargate/web/`:

```powershell
npm install
npm start          # dev server at http://localhost:4200
npm run build      # production build
npm test           # run unit tests with Vitest
npm run format     # format with Prettier
```

The dev server expects the API to be running separately at `http://localhost:5204` (configured via `environment.ts`).

---

## Guidance for agents

- **Follow existing patterns:** New use cases = new request + handler (and result type). Controllers only `Send` and `GetResponse`. Don't put business or data access in controllers.
- **Where to add code (API):** New queries → `Business/Queries/`. New commands → `Business/Commands/`. New response shapes → extend `BaseResponse` or add DTOs under `Domain/Dtos/`. New endpoints → add actions to the appropriate controller and a corresponding request/handler. New cross-cutting concerns → `Business/Services/` or `Business/Behaviors/`.
- **Logging:** All MediatR requests are automatically logged. No manual logging needed in handlers. If adding new pipeline concerns, follow the `LoggingBehavior` pattern using `IPipelineBehavior`.
- **Exception handling:** Throw domain exceptions (`NotFoundException`, `ConflictException`, `BadRequestException`, `UnprocessableEntityException`) from handlers. The middleware will convert them to appropriate HTTP responses and log them automatically.
- **Testing:** Services like `IApiLoggingService` can be mocked for unit tests. The MediatR split and service abstraction are intended to make testing easy.
- **Where to add code (frontend):** New API calls — extend `services/astronaut-api.service.ts` or add a new service. New views — new standalone component in `components/`. New models — add to `models/astronaut.model.ts`. New routes — `app.routes.ts` + a new component. Always use signals (`signal`, `computed`, `input`, `output`) and OnPush change detection. Never use `@Input`/`@Output` decorators.
- **API base URL:** Configured in `environments/environment.ts` (dev) and `environments/environment.prod.ts` (prod). Never hard-code the URL in services.
- **API responses use camelCase JSON** — match property names in `astronaut.model.ts` exactly when adding new interfaces.
