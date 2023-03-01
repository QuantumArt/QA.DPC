FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["QA.Core.DPC.Kafka.API/QA.Core.DPC.Kafka.API.csproj", "QA.Core.DPC.Kafka.API/"]
RUN dotnet restore "QA.Core.DPC.Kafka.API/QA.Core.DPC.Kafka.API.csproj"
COPY . .

FROM build AS publish
RUN dotnet publish "QA.Core.DPC.Kafka.API/QA.Core.DPC.Kafka.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QA.Core.DPC.Kafka.API.dll"]