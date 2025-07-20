# ShoppingApp

This repository contains a minimal skeleton for a shopping web application.
It includes:

- **Server**: an ASP.NET Core 8 Web API project with simple registration and login endpoints using an in-memory EF Core database.
- **client**: a placeholder React TypeScript project.

## Running the server

```bash
dotnet run --project Server/Server.csproj
```

The API exposes:
- `POST /api/register` – register a user with `FullName`, `Email`, and `Password`.
- `POST /api/login` – login with `Email` and `Password`.

Both endpoints return basic responses and do not implement JWT authentication yet.

## Client

The React client is only a placeholder and can be expanded to interact with the API.
