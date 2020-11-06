# Build temp image
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
LABEL stage=intermediate

RUN apt-get install -y \
    && curl -sL https://deb.nodesource.com/setup_12.x | bash \
    && apt-get install -y nodejs

WORKDIR /app
COPY . ./

WORKDIR /app/QA.ProductCatalog.Admin.WebApp
RUN npm ci
RUN npm run-script components:build-dev

RUN dotnet publish /app/QA.ProductCatalog.Admin.WebApp/QA.ProductCatalog.Admin.WebApp.csproj -c Release -o out -f netcoreapp3.1

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

ARG SERVICE_NAME
ENV SERVICE_NAME=${SERVICE_NAME:-Admin}

ARG SERVICE_VERSION
ENV SERVICE_VERSION=${SERVICE_VERSION:-0.0.0.0}

WORKDIR /app
COPY --from=build-env /app/QA.ProductCatalog.Admin.WebApp/out .
RUN rm -rf /app/hosting.json
ENTRYPOINT ["dotnet", "QA.ProductCatalog.Admin.WebApp.dll"]
