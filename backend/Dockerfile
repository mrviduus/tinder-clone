FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH
WORKDIR /src

# Copy project file and restore dependencies
COPY App/App.csproj App/
RUN dotnet restore App/App.csproj -a $TARGETARCH

# Copy source code and build
COPY App/ App/
RUN dotnet publish App/App.csproj -c Release -o /app/publish -a $TARGETARCH --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "App.dll"]