# Build temp image
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env

RUN apt-get install -y \
    && curl -sL https://deb.nodesource.com/setup_11.x | bash \
    && apt-get install -y nodejs

WORKDIR /app
COPY QA.DPC ./

RUN npm ci --prefix QA.ProductCatalog.Admin.WebApp --production
#RUN npm run-script build-prod --prefix QA.ProductCatalog.Admin.WebApp

RUN dotnet publish /app/QA.ProductCatalog.Admin.WebApp/QA.ProductCatalog.Admin.WebApp.csproj -c Release -o out -f netcoreapp2.2

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY --from=build-env /app/QA.ProductCatalog.Admin.WebApp/out .
ENTRYPOINT ["dotnet", "QA.ProductCatalog.Admin.WebApp.dll"]