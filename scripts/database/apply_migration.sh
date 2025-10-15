#!/bin/bash
# Apply Entity Framework migrations to the database
# Usage: ./apply_migration.sh

docker run --rm -v $(pwd)/../../backend:/src -w /src/App mcr.microsoft.com/dotnet/sdk:9.0 sh -c "
  dotnet tool install --global dotnet-ef
  export PATH=\"\$PATH:/root/.dotnet/tools\"
  dotnet ef database update --connection 'Host=host.docker.internal;Port=5432;Database=appdb;Username=appuser;Password=appsecret'
"