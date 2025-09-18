#!/bin/bash
docker run --rm -v $(pwd)/backend:/src -w /src/App mcr.microsoft.com/dotnet/sdk:8.0 sh -c "
  dotnet tool install --global dotnet-ef --version 8.0.8
  export PATH=\"\$PATH:/root/.dotnet/tools\"
  dotnet ef migrations add InitialCreateFixed
"