# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Ifolor.ConsumerService.Application/Ifolor.ConsumerService.Application.csproj", "src/Ifolor.ConsumerService.Application/"]
COPY ["src/Ifolor.ConsumerService.Infrastructure/Ifolor.ConsumerService.Infrastructure.csproj", "src/Ifolor.ConsumerService.Infrastructure/"]
COPY ["src/Ifolor.ConsumerService.Core/Ifolor.ConsumerService.Core.csproj", "src/Ifolor.ConsumerService.Core/"]
RUN dotnet restore "./src/Ifolor.ConsumerService.Application/Ifolor.ConsumerService.Application.csproj"
COPY . .
WORKDIR "/src/src/Ifolor.ConsumerService.Application"
RUN dotnet build "./Ifolor.ConsumerService.Application.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ifolor.ConsumerService.Application.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ifolor.ConsumerService.Application.dll"]