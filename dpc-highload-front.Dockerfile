# Build temp image
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
LABEL stage=intermediate

WORKDIR /app
COPY QA.Core.ProductCatalog.sln nuget.config ./
ADD projectfiles.tar .
RUN dotnet restore

COPY . ./

RUN dotnet publish /app/QA.ProductCatalog.HighloadFront.Core.API/QA.ProductCatalog.HighloadFront.Core.API.csproj -c Development -o out -f netcoreapp3.1 --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

ARG SERVICE_NAME
ENV SERVICE_NAME=${SERVICE_NAME:-HighloadFront}

ARG SERVICE_VERSION
ENV SERVICE_VERSION=${SERVICE_VERSION:-0.0.0.0}

WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "QA.ProductCatalog.HighloadFront.Core.API.dll"]  
