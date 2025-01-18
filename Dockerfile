# Base runtime image for .NET applications
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

EXPOSE 8085

# Build stage: SDK image with the .NET tools installed
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the .csproj file and restore dependencies
COPY YoutubeApiSyncronize.csproj ./
RUN dotnet restore "YoutubeApiSynchronize.csproj"

# Copy the rest of the application code and build
COPY . .
RUN dotnet build "YoutubeApiSynchronize.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "YoutubeApiSynchronize.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YoutubeApiSynchronize.dll"]