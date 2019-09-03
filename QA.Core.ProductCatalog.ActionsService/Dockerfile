# Build temp image
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env

WORKDIR /app
COPY QA.DPC ./

RUN dotnet publish /app/QA.Core.ProductCatalog.ActionsService/QA.Core.ProductCatalog.ActionsService.csproj -c Release -o out -f netcoreapp2.2

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY --from=build-env /app/QA.Core.ProductCatalog.ActionsService/out .
ENTRYPOINT ["dotnet", "QA.Core.ProductCatalog.ActionsService.dll"]