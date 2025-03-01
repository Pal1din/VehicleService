FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt update && apt install -y net-tools && apt install -y curl
ENV ASPNETCORE_URLS=http://+:80
USER $APP_UID
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VehicleService.API/VehicleService.API.csproj", "VehicleService.API/"]
COPY ["VehicleService.Grpc/VehicleService.Grpc.csproj", "VehicleService.Grpc/"]
COPY ["VehicleService.Data/VehicleService.Data.csproj", "VehicleService.Data/"]
RUN dotnet restore "VehicleService.API/VehicleService.API.csproj"
COPY . .
WORKDIR "/src/VehicleService.API"
RUN dotnet build "VehicleService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "VehicleService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VehicleService.API.dll"]
