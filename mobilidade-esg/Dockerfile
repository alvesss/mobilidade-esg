# syntax=docker/dockerfile:1
# ─────────────────────────────────────────────────────────────────────────────
# MobilidadeESG API — imagem multi-stage
#  1) build:   SDK .NET 8 restaura e publica (camada de restore fica em cache)
#  2) runtime: só o runtime ASP.NET, usuário sem privilégio, healthcheck
# ─────────────────────────────────────────────────────────────────────────────

# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia só o .csproj primeiro: se as dependências não mudarem, o restore vem do cache
COPY src/MobilidadeESG.csproj ./src/
RUN dotnet restore src/MobilidadeESG.csproj

COPY src/ ./src/
RUN dotnet publish src/MobilidadeESG.csproj -c Release -o /app/publish \
    --no-restore /p:UseAppHost=false

# ── Runtime stage ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# curl é usado apenas pelo HEALTHCHECK
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Versão da imagem (preenchida pelo pipeline com o SHA do commit)
ARG APP_VERSION=local
ENV APP_VERSION=${APP_VERSION} \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080

# Segredos (connection string, chave JWT) NÃO ficam na imagem:
# são injetados em tempo de execução via variáveis de ambiente.

EXPOSE 8080

# Usuário não-root que já vem na imagem oficial do .NET 8
USER app

HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=5 \
    CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "MobilidadeESG.dll"]
