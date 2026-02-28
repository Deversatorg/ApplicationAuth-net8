FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ApplicationAuth/ApplicationAuth.csproj", "ApplicationAuth/"]
COPY ["ApplicationAuth.Common/ApplicationAuth.Common.csproj", "ApplicationAuth.Common/"]
COPY ["ApplicationAuth.DAL/ApplicationAuth.DAL.csproj", "ApplicationAuth.DAL/"]
COPY ["ApplicationAuth.Domain/ApplicationAuth.Domain.csproj", "ApplicationAuth.Domain/"]
COPY ["ApplicationAuth.ResourceLibrary/ApplicationAuth.ResourceLibrary.csproj", "ApplicationAuth.ResourceLibrary/"]
COPY ["ApplicationAuth.ServiceDefaults/ApplicationAuth.ServiceDefaults.csproj", "ApplicationAuth.ServiceDefaults/"]
RUN dotnet restore "./ApplicationAuth/ApplicationAuth.csproj"
COPY . .
WORKDIR "/src/ApplicationAuth"
RUN dotnet build "./ApplicationAuth.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ApplicationAuth.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApplicationAuth.dll"]
