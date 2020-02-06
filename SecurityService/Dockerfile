FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["SecurityService/NuGet.Config", "."]
COPY ["SecurityService/SecurityService.csproj", "SecurityService/"]
COPY ["SecurityService.Manager/SecurityService.Manager.csproj", "SecurityService.Manager/"]
COPY ["SecurityService.DataTransferObjects/SecurityService.DataTransferObjects.csproj", "SecurityService.DataTransferObjects/"]

RUN dotnet restore "SecurityService/SecurityService.csproj"
COPY . .
WORKDIR "/src/SecurityService"
RUN dotnet build "SecurityService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SecurityService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SecurityService.dll"]