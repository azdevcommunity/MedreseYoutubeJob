# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8085
ENV ASPNETCORE_URLS=http://0.0.0.0:8085
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Build stage - CLEAN .NET 8.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Sadece proje dosyasını kopyala
COPY ["YoutubeApiSynchronize.csproj", "."]
# 2. Restore et
RUN dotnet restore "YoutubeApiSynchronize.csproj"

# 3. Tüm kodu kopyala
COPY . .
# 4. Build et
RUN dotnet build "YoutubeApiSynchronize.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "YoutubeApiSynchronize.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Logs klasörü oluştur
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "YoutubeApiSynchronize.dll"]