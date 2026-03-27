FROM stuartferguson/txnprocbase AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Certificates/*.*", "Certificates/"]
COPY ["SecurityService/NuGet.Config", "."]

COPY ["SecurityService/SecurityService.csproj", "SecurityService/"]
COPY ["SecurityService.BusinessLogic/SecurityService.BusinessLogic.csproj", "SecurityService.BusinessLogic/"]
COPY ["SecurityService.Database/SecurityService.Database.csproj", "SecurityService.Database/"]
COPY ["SecurityService.DataTransferObjects/SecurityService.DataTransferObjects.csproj", "SecurityService.DataTransferObjects/"]
COPY ["SecurityService.Models/SecurityService.Models.csproj", "SecurityService.Models/"]
COPY ["SecurityService.SqlServerMigrations/SecurityService.SqlServerMigrations.csproj", "SecurityService.SqlServerMigrations/"]

RUN dotnet restore "SecurityService/SecurityService.csproj"

COPY . .
WORKDIR /src/SecurityService
RUN dotnet publish "SecurityService.csproj" -c Release -o /app/publish /p:UseAppHost=false

WORKDIR /src/Certificates

# Sort out certificate stuff here
RUN cp aspnetapp-root-cert.cer /usr/local/share/ca-certificates/
RUN update-ca-certificates

WORKDIR /src/SecurityService
FROM build AS publish
RUN dotnet publish "SecurityService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
USER app
ENTRYPOINT ["dotnet", "SecurityService.dll"]
