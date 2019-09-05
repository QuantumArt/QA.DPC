# Build temp image
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env

WORKDIR /app
COPY QA.DPC ./

RUN dotnet publish /app/QA.Core.DPC.NotificationSender/QA.Core.DPC.NotificationSender.csproj -c Release -o out -f netcoreapp2.2

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2

ARG SERVICE_NAME
ENV SERVICE_NAME=${SERVICE_NAME:-NotificationSender}

ARG SERVICE_VERSION
ENV SERVICE_VERSION=${SERVICE_VERSION:-0.0.0.0}

WORKDIR /app
COPY --from=build-env /app/QA.Core.DPC.NotificationSender/out .
ENTRYPOINT ["dotnet", "QA.Core.DPC.NotificationSender.dll"]