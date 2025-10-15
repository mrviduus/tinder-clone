#!/bin/bash
# Run backend tests
# Usage: ./test.sh

echo "================================================"
echo "Running Tinder Clone API Tests"
echo "================================================"

# Navigate to project root and then to test directory
cd "$(dirname "$0")/../.."
cd backend/App.Tests

echo ""
echo "Restoring dependencies..."
dotnet restore

echo ""
echo "Building test project..."
dotnet build --no-restore

echo ""
echo "Running tests..."
dotnet test --no-build --verbosity normal --logger:"console;verbosity=detailed"

if [ $? -eq 0 ]; then
    echo ""
    echo "================================================"
    echo "✅ All tests passed successfully!"
    echo "================================================"
else
    echo ""
    echo "================================================"
    echo "❌ Some tests failed. Please review the output above."
    echo "================================================"
    exit 1
fi