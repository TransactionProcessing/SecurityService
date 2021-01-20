FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
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