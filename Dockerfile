# Build state
FROM mcr.microsoft.com/dotnet/sdk:8.0 as build-env

WORKDIR /App
COPY . .
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .
ENV ASPNETCORE_URLS=http://*:8080
ENTRYPOINT ["dotnet", "SmartIrrigatorAPI.dll"]
