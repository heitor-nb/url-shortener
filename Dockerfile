FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["UrlShortener.sln", "."]
COPY ["UrlShortener.Domain/UrlShortener.Domain.csproj", "UrlShortener.Domain/"]
COPY ["UrlShortener.Application/UrlShortener.Application.csproj", "UrlShortener.Application/"]
COPY ["UrlShortener.Infra/UrlShortener.Infra.csproj", "UrlShortener.Infra/"]
COPY ["UrlShortener.Api/UrlShortener.Api.csproj", "UrlShortener.Api/"]
COPY ["UrlShortener.Tests/UrlShortener.Tests.csproj", "UrlShortener.Tests/"]

RUN dotnet restore "UrlShortener.sln"

COPY . .

RUN dotnet publish "UrlShortener.Api/UrlShortener.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# WORKDIR /src

# COPY . .

# RUN dotnet publish "UrlShortener.Api/UrlShortener.Api.csproj" -c Release -o /app/publish

# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
# WORKDIR /app

# COPY --from=build /app/publish .

# EXPOSE 8080

# ENTRYPOINT ["dotnet", "SolarneApi.dll"]