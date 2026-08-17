FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/OpsFlow.Domain/OpsFlow.Domain.csproj src/OpsFlow.Domain/
COPY src/OpsFlow.Application/OpsFlow.Application.csproj src/OpsFlow.Application/
COPY src/OpsFlow.Infrastructure/OpsFlow.Infrastructure.csproj src/OpsFlow.Infrastructure/
COPY src/OpsFlow.Api/OpsFlow.Api.csproj src/OpsFlow.Api/
RUN dotnet restore src/OpsFlow.Api/OpsFlow.Api.csproj

COPY src/ src/
RUN dotnet publish src/OpsFlow.Api/OpsFlow.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "OpsFlow.Api.dll"]
