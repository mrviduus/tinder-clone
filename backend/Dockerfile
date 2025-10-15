FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY App/App.csproj App/
RUN dotnet restore App/App.csproj

# Copy source code and build
COPY App/ App/
RUN dotnet publish App/App.csproj -c Release -o /app/publish

# Runtime image
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "App.dll"]