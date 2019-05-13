FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers

COPY src/QA.Core.ProductCatalog.ActionsRunner QA.Core.ProductCatalog.ActionsRunner/
COPY src/QA.Core.ProductCatalog.ActionsRunnerModel QA.Core.ProductCatalog.ActionsRunnerModel/
COPY src/QA.Core.DPC.QP QA.Core.DPC.QP/
COPY src/QA.DPC.Core.Helpers QA.DPC.Core.Helpers/
COPY src/QA.ProductCatalog.ContentProviders QA.ProductCatalog.ContentProviders/
COPY src/QA.ProductCatalog.HighloadFront QA.ProductCatalog.HighloadFront/
COPY src/QA.ProductCatalog.HighloadFront.Elastic QA.ProductCatalog.HighloadFront.Elastic/
COPY src/QA.ProductCatalog.HighloadFront.Core.API QA.ProductCatalog.HighloadFront.Core.API/
COPY src/nuget.config /


RUN dotnet restore /app/QA.ProductCatalog.HighloadFront.Core.API/QA.ProductCatalog.HighloadFront.Core.API.csproj
RUN dotnet publish /app/QA.ProductCatalog.HighloadFront.Core.API/QA.ProductCatalog.HighloadFront.Core.API.csproj -r linux-x64 -c Release -o out 

# Build runtime image
FROM microsoft/dotnet:2.2-runtime
ARG service_name=dpc_highload_api
ARG service_version=0.1.1
ENV SERVICE_NAME=${service_name}
ENV SERVICE_VERSION=${service_version}
WORKDIR /app
COPY --from=build-env /app/QA.ProductCatalog.HighloadFront.Core.API/out .
ENTRYPOINT ["dotnet", "QA.ProductCatalog.HighloadFront.Core.API.dll"]