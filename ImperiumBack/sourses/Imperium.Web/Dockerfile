FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Imperium.Web/Imperium.Web.csproj", "Imperium.Web/"]
COPY ["Imperium.Service/Imperium.Service.csproj", "Imperium.Service/"]
COPY ["Imperium.Data/Imperium.Data.csproj", "Imperium.Data/"]
COPY ["Imperium.Core/Imperium.Core.csproj", "Imperium.Core/"]
RUN dotnet restore "Imperium.Web/Imperium.Web.csproj"
COPY . .
WORKDIR /src/Imperium.Web
RUN dotnet build "Imperium.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/Imperium.Web
RUN dotnet publish "Imperium.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Imperium.Web.dll"]