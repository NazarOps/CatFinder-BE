# SEEKAT — CatFinder Backend

A REST API for a lost and found cat matching platform. Users can post advertisements for lost or found cats, register cat details, comment on posts, and bookmark ads they want to follow.

## Tech Stack

- **Framework:** ASP.NET Core 10.0 (Web API)
- **Database:** SQL Server + Entity Framework Core 10.0
- **Architecture:** Clean Architecture (4-layer) with CQRS via MediatR
- **Authentication:** JWT with refresh tokens
- **Validation:** FluentValidation
- **Mapping:** AutoMapper
- **Docs:** Swagger / OpenAPI

## Project Structure

```
catfinder-BEe/
├── APILayer/              # Controllers, middleware, startup
├── ApplicationLayer/      # CQRS handlers, DTOs, validators, mappings
├── DomainLayer/           # Entities, enums, core business rules
└── InfrastructureLayer/   # EF Core, repositories, auth services, migrations
```

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB works for local dev)

## Getting Started

**1. Clone the repository**

```bash
git clone <repo-url>
cd catfinder-BEe
```

**2. Restore dependencies**

```bash
dotnet restore
```

**3. Configure the database**

Update `APILayer/appsettings.json` with your connection string if needed (LocalDB is the default):

```json
"ConnectionStrings": {
  "CatFinderDb": "Server=(localdb)\\mssqllocaldb;Database=CatFinderDb;Trusted_Connection=True;"
}
```

**4. Apply migrations**

```bash
dotnet ef database update --project InfrastructureLayer --startup-project APILayer
```

**5. Run the API**

```bash
cd APILayer
dotnet run
```

The API starts at:
- HTTP: `http://localhost:5051`
- HTTPS: `https://localhost:7050`

Swagger UI is available at `http://localhost:5051/swagger` in development.

## Configuration

All settings live in `APILayer/appsettings.json`.

| Setting | Default | Description |
|---|---|---|
| `JwtSettings:SecretKey` | `catfinder-development-secret-key-...` | JWT signing key — **change in production** |
| `JwtSettings:ExpiresInMinutes` | `60` | Access token lifetime |
| `JwtSettings:RefreshTokenExpiresInDays` | `7` | Refresh token lifetime |
| `RateLimiting:Global:PermitLimit` | `100` | Requests per 60-second window |
| `RateLimiting:Auth:PermitLimit` | `10` | Auth endpoint limit per 60-second window |
| `CorsOrigins` | `http://localhost:5173`, `http://localhost:3000` | Allowed frontend origins |

## API Endpoints

### Auth
| Method | Endpoint | Auth required |
|---|---|---|
| POST | `/api/auth/register` | No |
| POST | `/api/auth/login` | No |
| POST | `/api/auth/refresh-token` | No |
| POST | `/api/auth/logout` | Yes |
| POST | `/api/auth/reset-password` | No |

### Advertisements
| Method | Endpoint | Auth required |
|---|---|---|
| GET | `/api/advertisements` | No |
| GET | `/api/advertisements/{id}` | No |
| POST | `/api/advertisements` | Yes |
| PUT | `/api/advertisements/{id}` | Yes |
| PUT | `/api/advertisements/{id}/status` | Yes |
| GET | `/api/advertisements/my` | Yes |
| GET | `/api/advertisements/admin` | Admin |
| PUT | `/api/advertisements/{id}/moderation-status` | Admin |
| DELETE | `/api/advertisements/{id}` | Yes |

Query params for listing: `type` (`Lost` or `Found`), `city`

### Cats
| Method | Endpoint | Auth required |
|---|---|---|
| GET | `/api/cats/{id}` | No |
| POST | `/api/cats` | Yes |
| PUT | `/api/cats/{id}` | Yes |
| DELETE | `/api/cats/{id}` | Yes |

### Comments
| Method | Endpoint | Auth required |
|---|---|---|
| GET | `/api/comments?advertisementId={id}` | No |
| POST | `/api/comments` | Yes |
| PUT | `/api/comments/{id}` | Yes |
| DELETE | `/api/comments/{id}` | Yes |

### Saved Advertisements
| Method | Endpoint | Auth required |
|---|---|---|
| GET | `/api/savedadvertisements` | Yes |
| GET | `/api/savedadvertisements/{id}` | Yes |
| POST | `/api/savedadvertisements` | Yes |
| DELETE | `/api/savedadvertisements/{id}` | Yes |

### Other
- `GET /api/locations` — List locations
- `POST /api/locations` — Create location
- `GET /api/accounts/{id}` — Account details
- `POST /api/advertisementimages` — Upload image
- `GET /health` — Health check

## Running Tests

```bash
dotnet test
```

## Environment Variables

Sensitive values should not live in `appsettings.json` in production. Override them via environment variables or a secrets manager.

Create an `appsettings.Production.json` or set environment variables directly:

```bash
# JWT
JwtSettings__SecretKey=your-strong-secret-key-here
JwtSettings__Issuer=CatFinderAPI
JwtSettings__Audience=CatFinderClient
JwtSettings__ExpiresInMinutes=60
JwtSettings__RefreshTokenExpiresInDays=7

# Database
ConnectionStrings__CatFinderDb=Server=your-server;Database=CatFinderDb;User Id=...;Password=...;

# CORS
CorsOrigins=https://your-frontend-domain.com
```

ASP.NET Core automatically picks up environment variables that match the config key structure (double underscore `__` replaces `:` as the separator).

## Related

- **Frontend:** *(link to frontend repo)*

## Contributing

1. Fork the repository and create a branch from `main`
2. Branch naming: `feature/short-description`, `bugfix/short-description`, or `hotfix/short-description`
3. Keep commits focused — one logical change per commit
4. Open a pull request against `main` with a clear description of what changed and why
5. Make sure the project builds and tests pass before requesting a review (`dotnet build` + `dotnet test`)
