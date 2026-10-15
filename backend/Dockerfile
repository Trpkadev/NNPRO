FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend.csproj", "./"]
RUN dotnet restore "backend.csproj"
COPY . .
RUN dotnet build "backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Generování certifikátu pro Kestrel
USER root
RUN apt-get update && apt-get install -y openssl && rm -rf /var/lib/apt/lists/*
RUN mkdir -p /https && \
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
      -keyout /https/aspnetapp.key \
      -out /https/aspnetapp.crt \
      -subj "/CN=localhost" && \
    openssl pkcs12 -export -out /https/aspnetapp.pfx \
      -inkey /https/aspnetapp.key \
      -in /https/aspnetapp.crt \
      -passout pass:nnpro && \
    chown -R $APP_UID:$APP_UID /https

COPY --from=publish /app/publish .
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS="8080"
ENV ASPNETCORE_HTTPS_PORTS="8081"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/https/aspnetapp.pfx"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="nnpro"

ENTRYPOINT ["dotnet", "backend.dll"]