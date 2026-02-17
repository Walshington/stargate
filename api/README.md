# Stargate API

How to get the API running locally.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Setup & run

### 1. Restore packages

From the `api` directory:

```powershell
dotnet restore
```

### 2. Create / update the database

The app uses a local SQLite database. Apply migrations to create or update it:

```powershell
dotnet ef database update
```

This creates (or updates) `starbase.db` in the api folder.

### 3. Start the API

```powershell
dotnet run
```

The API will listen at **http://localhost:5204** (or the port shown in the console). Open **http://localhost:5204/swagger** in a browser to view and test the endpoints.

## Development

### Testing

API unit tests live in the `tests/Stargate.Api.Tests` project and are run from the solution root using the `Stargate.sln` solution file.

From the `stargate` directory (solution root):

```powershell
dotnet test
```

This will build the API and execute all tests in `Stargate.Api.Tests`.

### Formatting

Format the codebase using the built-in .NET formatter (respects `.editorconfig` rules):

```powershell
dotnet format
```

Check formatting without making changes (useful for CI/CD):

```powershell
dotnet format --verify-no-changes
```

### Linting & Analysis

Run static analysis and check for warnings:

```powershell
dotnet build
```

## Logging

The API implements comprehensive database-backed logging that captures all requests, responses, and exceptions for audit and debugging purposes.

### How It Works

**Automatic Request/Response Logging:**
- Every MediatR command and query is automatically logged via `LoggingBehavior` pipeline behavior
- Captures request data, response data, execution duration, and HTTP status codes
- No manual logging code needed in handlers

**Exception Logging:**
- MediatR handler exceptions logged by `LoggingBehavior` with full request context
- HTTP-level exceptions logged by `ExceptionHandlingMiddleware` with HTTP context (method, path, error response)
- Pre-processor validation exceptions only logged at HTTP level (MediatR limitation)

**Dual Output:**
- **Console/Debug:** All requests and exceptions logged via ASP.NET Core's `ILogger` for real-time monitoring
- **Database:** All logs persisted to `ApiLog` table for queryable audit trail

**Transaction Isolation:**
- Logging uses separate `DbContext` instances to ensure logs persist even when business transactions roll back
- SQLite WAL mode enabled for concurrent write support

### Querying Logs

Logs are stored in the `ApiLog` table with the following key fields:
- `RequestType`: MediatR request name (e.g., "CreatePerson") or "HTTP_REQUEST" for middleware logs
- `RequestData`: Serialized request object (JSON)
- `ResponseData`: Serialized response object (JSON)
- `StatusCode`: HTTP status code
- `DurationMs`: Execution time in milliseconds
- `ExceptionMessage` / `ExceptionStackTrace`: Present for error logs
- `Level`: "Information" for success, "Error" for exceptions
