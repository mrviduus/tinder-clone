#!/bin/bash

# Wait for database to be ready
echo "Waiting for database to be ready..."
until docker-compose exec db pg_isready -U appuser -d appdb; do
  echo "Database is not ready yet. Waiting..."
  sleep 2
done

echo "Database is ready!"

# Apply migrations
echo "Applying database migrations..."
docker-compose exec api dotnet ef database update --no-build --verbose

echo "Database initialization complete!"