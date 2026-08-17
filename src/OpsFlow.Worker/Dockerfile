FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/OpsFlow.Domain/OpsFlow.Domain.csproj src/OpsFlow.Domain/
COPY src/OpsFlow.Application/OpsFlow.Application.csproj src/OpsFlow.Application/
COPY src/OpsFlow.Infrastructure/OpsFlow.Infrastructure.csproj src/OpsFlow.Infrastructure/
COPY src/OpsFlow.Worker/OpsFlow.Worker.csproj src/OpsFlow.Worker/
RUN dotnet restore src/OpsFlow.Worker/OpsFlow.Worker.csproj

COPY src/ src/
RUN dotnet publish src/OpsFlow.Worker/OpsFlow.Worker.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "OpsFlow.Worker.dll"]
