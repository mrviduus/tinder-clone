#!/bin/bash
# Create a new Entity Framework migration
# Usage: ./create_migration.sh <MigrationName>

MIGRATION_NAME=${1:-"NewMigration"}

docker run --rm -v $(pwd)/../../backend:/src -w /src/App mcr.microsoft.com/dotnet/sdk:9.0 sh -c "
  dotnet tool install --global dotnet-ef
  export PATH=\"\$PATH:/root/.dotnet/tools\"
  dotnet ef migrations add $MIGRATION_NAME
"