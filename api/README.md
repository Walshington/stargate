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
