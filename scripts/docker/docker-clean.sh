#!/bin/bash

# Docker Clean Script for Tinder Clone
# This script performs a complete cleanup of Docker resources for the project

set -e  # Exit on error

echo "🧹 Starting Docker cleanup for Tinder Clone..."
echo "⚠️  WARNING: This will remove all containers, images, volumes, and networks for this project!"
echo ""

# Check if user wants to proceed
read -p "Are you sure you want to continue? (y/N): " -n 1 -r
echo ""
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "❌ Cleanup cancelled."
    exit 1
fi

# Navigate to project root
PROJECT_ROOT="$(dirname "$0")/../.."
cd "$PROJECT_ROOT"

echo ""
echo "📦 Step 1: Stopping all running containers..."
docker-compose down 2>/dev/null || true

echo ""
echo "🗑️  Step 2: Removing containers with volumes..."
docker-compose down -v 2>/dev/null || true

echo ""
echo "🧹 Step 3: Removing project-specific containers..."
docker ps -a --filter "name=tinder-clone" --format "{{.ID}}" | xargs -r docker rm -f 2>/dev/null || true

echo ""
echo "🖼️  Step 4: Removing project-specific images..."
docker images --filter "reference=tinder-clone*" --format "{{.ID}}" | xargs -r docker rmi -f 2>/dev/null || true

echo ""
echo "💾 Step 5: Removing project-specific volumes..."
docker volume ls --filter "name=tinder-clone" --format "{{.Name}}" | xargs -r docker volume rm 2>/dev/null || true

echo ""
echo "🌐 Step 6: Removing project-specific networks..."
docker network ls --filter "name=tinder-clone" --format "{{.Name}}" | xargs -r docker network rm 2>/dev/null || true

echo ""
echo "🔍 Step 7: Pruning dangling images..."
docker image prune -f

echo ""
echo "🔍 Step 8: Pruning dangling volumes..."
docker volume prune -f

echo ""
echo "📊 Step 9: Showing Docker disk usage..."
docker system df

echo ""
echo "✅ Docker cleanup completed!"
echo ""
echo "To perform a system-wide Docker cleanup (removes ALL unused Docker resources), run:"
echo "  docker system prune -a --volumes"
echo ""
echo "To rebuild the project from scratch, run:"
echo "  docker-compose up --build"
echo ""
echo "To rebuild with migrations, run:"
echo "  docker-compose --profile migration up --build"