# ShoppingApp

This repository contains a minimal skeleton for a shopping web application.
It includes:

- **Server**: an ASP.NET Core 8 Web API project with registration, login and basic profile endpoints using an in-memory EF Core database and JWT authentication.
- **client**: a placeholder React TypeScript project.

## Running the server

```bash
dotnet run --project Server/Server.csproj
```

The API exposes:
- `POST /api/register` – register a user with `FullName`, `Email`, and `Password`.
- `POST /api/login` – login with `Email` and `Password` and receive a JWT token.
- `GET /api/profile` – get the authenticated user's profile (requires JWT).
- `PUT /api/profile` – update the authenticated user's profile (requires JWT).
- `POST /api/change-password` – change the authenticated user's password (requires JWT).

## Client

The React client is only a placeholder and can be expanded to interact with the API.
