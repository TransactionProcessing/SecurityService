FROM stuartferguson/txnprocbase:2024.8.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the ARG for your GitHub Secret
ARG NUGET_TOKEN

WORKDIR /src
COPY ["Certificates/*.*", "Certificates/"]
COPY ["SecurityService/NuGet.Config", "."]
# Replace the placeholders in the NuGet.config file with the GitHub Secret
RUN sed -i "s|NUGET_TOKEN|${NUGET_TOKEN}|g" NuGet.Config

COPY ["SecurityService/SecurityService.csproj", "SecurityService/"]
COPY ["SecurityService.BusinessLogic/SecurityService.BusinessLogic.csproj", "SecurityService.BusinessLogic/"]
COPY ["SecurityService.DataTransferObjects/SecurityService.DataTransferObjects.csproj", "SecurityService.DataTransferObjects/"]

RUN dotnet restore "SecurityService/SecurityService.csproj"
COPY . .
WORKDIR /src/SecurityService
RUN dotnet build "SecurityService.csproj" -c Release -o /app

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
ENTRYPOINT ["dotnet", "SecurityService.dll"]