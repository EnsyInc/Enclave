# syntax=docker/dockerfile:1

# Builds both the Api and the one-shot Migrations runner from a single multi-stage Dockerfile.
# Build context must be src/EnsyInc.Enclave (see repo-root docker-compose.yml).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore EnsyInc.Enclave.slnx

# dotnet dev-certs is SDK-only tooling; generate the HTTPS dev cert here so it can be copied into
# the runtime-only Api image below. The cert only needs to exist for Kestrel to serve HTTPS with -
# EnsyInc.Enclave.ServiceTests already bypasses server cert validation on the client side, so
# trusting the cert on the host/container isn't required.
ARG DEV_CERT_PASSWORD=DevCertPass123!
RUN mkdir -p /https && dotnet dev-certs https -ep /https/aspnetcore.pfx -p "$DEV_CERT_PASSWORD"

FROM build AS publish-api
RUN dotnet publish EnsyInc.Enclave.Api/EnsyInc.Enclave.Api.csproj -c Release -o /app/publish --no-restore

FROM build AS publish-migrations
RUN dotnet publish EnsyInc.Enclave.Migrations/EnsyInc.Enclave.Migrations.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
WORKDIR /app
ARG DEV_CERT_PASSWORD=DevCertPass123!
COPY --from=publish-api /app/publish .
COPY --from=build /https/aspnetcore.pfx /https/aspnetcore.pfx
ENV ASPNETCORE_URLS="https://+:7150;http://+:5243" \
    ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetcore.pfx \
    ASPNETCORE_Kestrel__Certificates__Default__Password=$DEV_CERT_PASSWORD
EXPOSE 7150 5243
ENTRYPOINT ["dotnet", "EnsyInc.Enclave.Api.dll"]

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS migrations
WORKDIR /app
COPY --from=publish-migrations /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "EnsyInc.Enclave.Migrations.dll"]
