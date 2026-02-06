# Stage 1: Build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj and restore (caching layer)
COPY *.csproj ./
RUN dotnet restore

# Copy the rest and publish
COPY . ./
RUN dotnet publish -c Release -o out --no-restore

# Stage 2: Runtime image (lighter, only what's needed to run)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Render sets PORT env var automatically — your app should listen on it
# (most ASP.NET Core apps do this by default in .NET 6+)
EXPOSE 8080

# Entry point — replace "ServePoint.Cadet.dll" with your actual published DLL name if different
ENTRYPOINT ["dotnet", "ServePoint.Cadet.dll"]