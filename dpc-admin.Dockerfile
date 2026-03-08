# Build temp image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
LABEL stage=intermediate

RUN apt-get update -y \
    && apt-get install -y curl \
    && curl -fsSL https://deb.nodesource.com/setup_24.x | bash - \
    && apt-get install -y nodejs \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY QA.Core.ProductCatalog.sln nuget.config ./
ADD projectfiles.tar .
RUN dotnet restore

WORKDIR /app/QA.ProductCatalog.Admin.WebApp
COPY QA.ProductCatalog.Admin.WebApp/package*.json ./
RUN npm ci

WORKDIR /app
COPY . ./

WORKDIR /app/QA.ProductCatalog.Admin.WebApp
RUN npm run components:build-prod

RUN dotnet publish /app/QA.ProductCatalog.Admin.WebApp/QA.ProductCatalog.Admin.WebApp.csproj -c Release -o out -f net8.0 --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

ARG SERVICE_NAME
ENV SERVICE_NAME=${SERVICE_NAME:-Admin}

ARG SERVICE_VERSION
ENV SERVICE_VERSION=${SERVICE_VERSION:-0.0.0.0}

WORKDIR /app
COPY --from=build-env /app/QA.ProductCatalog.Admin.WebApp/out .
ENTRYPOINT ["dotnet", "QA.ProductCatalog.Admin.WebApp.dll", "--urls", "http://*:80"]
