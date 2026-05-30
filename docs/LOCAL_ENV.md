# Local Development Environment

## Machine
- OS: Ubuntu 25.10
- LAN IP: `192.168.110.16`
- Tailscale IP: `100.122.169.61`

## Installed Runtime
- .NET SDK: `10.0.108`
- ASP.NET Core Runtime: `10.0.8`
- PostgreSQL: `17.10`

## Database
- Host: `localhost`
- Port: `5432`
- Database: `xparf`
- User: `postgres`
- Password: configured locally for development only.

## Build Commands
```bash
dotnet restore xparf.slnx
dotnet build xparf.slnx
dotnet test xparf.slnx
```

## Run API on LAN
```bash
ASPNETCORE_ENVIRONMENT=Development \
ASPNETCORE_URLS=http://0.0.0.0:5000 \
dotnet src/Xparf.Api/bin/Debug/net10.0/Xparf.Api.dll
```

Health check:
```bash
curl http://127.0.0.1:5000/api/health
```

LAN URL:
```text
http://192.168.110.16:5000/api/health
```

## Current Env Status
- `dotnet restore` succeeds.
- `dotnet build` succeeds.
- API health endpoint returns `200 OK`.
- No EF migration exists yet.
- Build warning remains: `NU1903 Microsoft.Build.Tasks.Core 17.7.2` high severity advisory via transitive dependency.
