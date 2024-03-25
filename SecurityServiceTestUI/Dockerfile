FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the ARG for your GitHub Secret
ARG NUGET_TOKEN

WORKDIR /src
COPY ["Certificates/*.*", "Certificates/"]
COPY ["SecurityService/NuGet.Config", "."]
# Replace the placeholders in the NuGet.config file with the GitHub Secret
RUN sed -i "s|NUGET_TOKEN|${NUGET_TOKEN}|g" NuGet.Config

COPY ["Certificates/*.*", "Certificates/"]
COPY ["SecurityServiceTestUI/SecurityServiceTestUI.csproj", "SecurityServiceTestUI/"]
RUN dotnet restore "SecurityServiceTestUI/SecurityServiceTestUI.csproj"
COPY . .
WORKDIR "/src/SecurityServiceTestUI"
RUN dotnet build "SecurityServiceTestUI.csproj" -c Release -o /app/build

# Sort out certificate stuff here
WORKDIR /src/Certificates
# Sort out certificate stuff here
RUN cp aspnetapp-root-cert.cer /usr/local/share/ca-certificates/
RUN update-ca-certificates

WORKDIR /src/SecurityServiceTestUI
FROM build AS publish
RUN dotnet publish "SecurityServiceTestUI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SecurityServiceTestUI.dll"]