FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CloverleafTrack.Web/CloverleafTrack.Web.csproj", "CloverleafTrack.Web/"]
COPY ["CloverleafTrack.SearchGenerator/CloverleafTrack.SearchGenerator.csproj", "CloverleafTrack.SearchGenerator/"]
COPY ["CloverleafTrack.Services/CloverleafTrack.Services.csproj", "CloverleafTrack.Services/"]
COPY ["CloverleafTrack.DataAccess/CloverleafTrack.DataAccess.csproj", "CloverleafTrack.DataAccess/"]
COPY ["CloverleafTrack.Models/CloverleafTrack.Models.csproj", "CloverleafTrack.Models/"]
COPY ["CloverleafTrack.ViewModels/CloverleafTrack.ViewModels.csproj", "CloverleafTrack.ViewModels/"]
RUN dotnet restore "CloverleafTrack.Web/CloverleafTrack.Web.csproj"
RUN dotnet restore "CloverleafTrack.SearchGenerator/CloverleafTrack.SearchGenerator.csproj"
COPY . .
WORKDIR "/src/CloverleafTrack.Web"
RUN dotnet build "./CloverleafTrack.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish-web
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CloverleafTrack.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build AS publish-generator
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "/src/CloverleafTrack.SearchGenerator/CloverleafTrack.SearchGenerator.csproj" -c $BUILD_CONFIGURATION -o /app/generator /p:UseAppHost=false

# Run the generator when SEARCH_DB_CONNECTION is provided; silently skip otherwise.
FROM publish-web AS with-search-index
COPY --from=publish-generator /app/generator /app/generator
ARG SEARCH_DB_CONNECTION=""
RUN if [ -n "$SEARCH_DB_CONNECTION" ]; then \
      dotnet /app/generator/CloverleafTrack.SearchGenerator.dll \
        "$SEARCH_DB_CONNECTION" /app/publish/wwwroot/search-index.json \
      || echo "Warning: search index generation failed — the dynamic fallback will be used at runtime."; \
    fi

FROM base AS final
WORKDIR /app
COPY --from=with-search-index /app/publish .
ENTRYPOINT ["dotnet", "CloverleafTrack.Web.dll"]
