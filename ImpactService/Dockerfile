FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY QA.ProductCatalog.ImpactService QA.ProductCatalog.ImpactService/
COPY QA.ProductCatalog.ImpactService.API QA.ProductCatalog.ImpactService.API/

RUN dotnet restore /app/QA.ProductCatalog.ImpactService.API/QA.ProductCatalog.ImpactService.API.csproj
RUN dotnet publish /app/QA.ProductCatalog.ImpactService.API/QA.ProductCatalog.ImpactService.API.csproj -r linux-x64 -c Release -o out 

# Build runtime image
FROM microsoft/dotnet:2.2-runtime
ARG service_name=dpc_impact
ARG service_version=0.1.1
ENV SERVICE_NAME=${service_name}
ENV SERVICE_VERSION=${service_version}
WORKDIR /app
COPY --from=build-env /app/QA.ProductCatalog.ImpactService.API/out .
ENTRYPOINT ["dotnet", "QA.ProductCatalog.ImpactService.API.dll"]