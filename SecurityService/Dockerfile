FROM stuartferguson/txnprocbase AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SecurityService/NuGet.Config", "."]
COPY ["SecurityService/SecurityService.csproj", "SecurityService/"]
COPY ["SecurityService.BusinessLogic/SecurityService.BusinessLogic.csproj", "SecurityService.BusinessLogic/"]
COPY ["SecurityService.DataTransferObjects/SecurityService.DataTransferObjects.csproj", "SecurityService.DataTransferObjects/"]

RUN dotnet restore "SecurityService/SecurityService.csproj"
COPY . .
WORKDIR "/src/SecurityService"
RUN dotnet build "SecurityService.csproj" -c Release -o /app

# Sort out certificate stuff here
RUN openssl x509 -inform DER -in /src/SecurityService/aspnetapp-root-cert.cer -out /src/SecurityService/aspnetapp-root-cert.crt
RUN cp /src/SecurityService/aspnetapp-root-cert.crt /usr/local/share/ca-certificates/
RUN update-ca-certificates

FROM build AS publish
RUN dotnet publish "SecurityService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SecurityService.dll"]