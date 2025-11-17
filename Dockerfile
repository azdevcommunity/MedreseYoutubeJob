# Base runtime image for .NET applications
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Uygulama 8085 dinleyecekse:
ENV ASPNETCORE_URLS=http://0.0.0.0:8085
# Serilog console output'unu buffer'lamadan göster
ENV DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION=true
ENV DOTNET_RUNNING_IN_CONTAINER=true


# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Sadece csproj kopyala ve restore yap (cache verimli)
COPY YoutubeApiSynchronize.csproj ./
RUN dotnet restore "YoutubeApiSynchronize.csproj"

# Kodları kopyala ve build et
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

# appsettings dosyalarını kopyala
COPY appsettings.json .
COPY appsettings.dev.json .

# Logs klasörü oluştur
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "YoutubeApiSynchronize.dll"]
