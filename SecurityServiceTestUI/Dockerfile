#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SecurityServiceTestUI/SecurityServiceTestUI.csproj", "SecurityServiceTestUI/"]
RUN dotnet restore "SecurityServiceTestUI/SecurityServiceTestUI.csproj"
COPY . .
WORKDIR "/src/SecurityServiceTestUI"
RUN dotnet build "SecurityServiceTestUI.csproj" -c Release -o /app/build

# Sort out certificate stuff here
RUN openssl x509 -inform DER -in /src/SecurityServiceTestUI/aspnetapp-root-cert.cer -out /src/SecurityServiceTestUI/aspnetapp-root-cert.crt
RUN cp /src/SecurityServiceTestUI/aspnetapp-root-cert.crt /usr/local/share/ca-certificates/
RUN update-ca-certificates

FROM build AS publish
RUN dotnet publish "SecurityServiceTestUI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SecurityServiceTestUI.dll"]