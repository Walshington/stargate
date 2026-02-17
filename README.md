# Stargate - Astronaut Career Tracking System (ACTS)

A full-stack application for tracking astronaut careers, duties, and service history.

## Overview

Stargate (ACTS) maintains comprehensive records of astronauts and their career progression, including:
- Personal information and current status
- Duty assignments with ranks and titles
- Career timelines (start dates, end dates, retirement)
- Historical duty records

## Architecture

```
stargate/
├── api/          # .NET 8 REST API (Backend)
├── web/          # Angular 21 SPA (Frontend)
└── tests/        # API test suite
```

### Backend ([api/](api/))
- **.NET 8** REST API with **MediatR CQRS** pattern
- **Entity Framework Core** with SQLite database
- **Dapper** for optimized queries
- **Swagger** documentation
- Automatic request/response logging to database

**Setup:** See [api/README.md](api/README.md)

### Frontend ([web/](web/))
- **Angular 21** with standalone components
- **Signal-based reactivity** for optimal performance
- **OnPush change detection** strategy
- **TypeScript** with strict mode
- **SCSS** styling

**Setup:** See [web/README.md](web/README.md)

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Angular 21, TypeScript, RxJS, SCSS |
| Backend | .NET 8, C#, ASP.NET Core |
| Data Access | Entity Framework Core, Dapper |
| Database | SQLite |
| Testing | xUnit, Vitest |
| API Documentation | Swagger/OpenAPI |

## Project Structure

### API Structure (`api/`)
```
api/
├── Controllers/          # HTTP endpoints (routing only)
├── Business/
│   ├── Commands/        # Write operations (Create, Update)
│   ├── Queries/         # Read operations (Get, List)
│   ├── Data/           # EF Core context, entities, migrations
│   ├── Behaviors/      # MediatR pipeline (logging)
│   └── Services/       # Cross-cutting concerns (API logging)
├── Domain/             # Shared contracts, DTOs, exceptions
└── Middleware/         # Exception handling, cross-cutting concerns
```

### Frontend Structure (`web/src/app/`)
```
app/
├── components/
│   ├── astronaut-search/      # Search bar component
│   └── astronaut-duty-list/   # Duty history display
├── models/                     # TypeScript interfaces
├── services/                   # API communication
└── environments/              # Configuration files
```

## Development Guidelines

### API Patterns
- **Controllers**: HTTP only, delegate to MediatR
- **Handlers**: Business logic, data access
- **Request/Response**: All operations use MediatR request types
- **Logging**: Automatic via `LoggingBehavior` pipeline
- **Exceptions**: Domain exceptions converted to HTTP responses by middleware

### Frontend Patterns
- **Standalone Components**: No NgModules
- **Signals**: Reactive state management
- **OnPush**: Change detection optimization
- **Computed Signals**: Derived state
- **Protected Members**: Template-accessible properties/methods
- **takeUntilDestroyed**: Automatic subscription cleanup

## Documentation

- **API Documentation**: See [api/README.md](api/README.md) for setup, endpoints, and API details
- **Frontend Documentation**: See [web/README.md](web/README.md) for setup and development
- **Agent Context**: See [AGENTS.md](AGENTS.md) for AI/contributor guidance
