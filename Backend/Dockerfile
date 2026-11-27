FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Backend.csproj", "./"]
RUN dotnet restore "Backend.csproj"
COPY . .
RUN dotnet build "Backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

USER root
RUN mkdir -p /https /home/app/dp_keys && \
    chown -R $APP_UID:$APP_UID /https /home/app

COPY --from=publish /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "Backend.dll"]

FROM final AS development
ENV ASPNETCORE_ENVIRONMENT="Development"
ENV ASPNETCORE_HTTP_PORTS="8080"
ENV ASPNETCORE_HTTPS_PORTS="8081"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/https/aspnetapp.pfx"

FROM final AS production
ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ASPNETCORE_HTTP_PORTS="8080"
ENV ASPNETCORE_HTTPS_PORTS=""