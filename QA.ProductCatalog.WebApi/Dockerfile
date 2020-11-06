# Build temp image
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /app
COPY . ./

RUN dotnet publish /app/QA.ProductCatalog.WebApi/QA.ProductCatalog.WebApi.csproj -c Release -o out -f netcoreapp3.1

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

ARG SERVICE_NAME
ENV SERVICE_NAME=${SERVICE_NAME:-WebApi}

ARG SERVICE_VERSION
ENV SERVICE_VERSION=${SERVICE_VERSION:-0.0.0.0}

WORKDIR /app
COPY --from=build-env /app/out .
RUN rm -rf /app/hosting.json
ENTRYPOINT ["dotnet", "QA.ProductCatalog.WebApi.dll"]