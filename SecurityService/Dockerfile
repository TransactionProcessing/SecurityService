FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["SecurityService.Service/NuGet.Config", "."]
COPY ["SecurityService.Service/SecurityService.Service.csproj", "SecurityService.Service/"]
COPY ["SecurityService.Service/NuGet.Config", "SecurityService.Service/"]
COPY ["SecurityService.Manager/SecurityService.Manager.csproj", "SecurityService.Manager/"]
COPY ["SecurityService.DataTransferObjects/SecurityService.DataTransferObjects.csproj", "SecurityService.Manager/"]

RUN dotnet restore "SecurityService.Service/SecurityService.Service.csproj"
COPY . .
WORKDIR "/src/SecurityService.Service"
RUN dotnet build "SecurityService.Service.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SecurityService.Service.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SecurityService.Service.dll"]