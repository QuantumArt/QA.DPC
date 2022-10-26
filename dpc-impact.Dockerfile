# Build temp image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /app
COPY QA.Core.ProductCatalog.sln nuget.config ./
ADD projectfiles.tar .
RUN dotnet restore

COPY . ./

RUN dotnet publish /app/ImpactService/QA.ProductCatalog.ImpactService.API/QA.ProductCatalog.ImpactService.API.csproj -c Release -o out -f net6.0 --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0

ARG SERVICE_NAME
ENV SERVICE_NAME=${SERVICE_NAME:-ImpactService}

ARG SERVICE_VERSION
ENV SERVICE_VERSION=${SERVICE_VERSION:-0.0.0.0}

WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "QA.ProductCatalog.ImpactService.API.dll"]
