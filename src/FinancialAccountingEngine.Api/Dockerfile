# syntax=docker/dockerfile:1
# Multi-stage build for the Financial Accounting Engine API.
# Build context is the repository root (see docker-compose.yml).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore: copy only the project files first to maximize Docker layer caching.
COPY Directory.Build.props ./
COPY src/FinancialAccountingEngine.Domain/FinancialAccountingEngine.Domain.csproj src/FinancialAccountingEngine.Domain/
COPY src/FinancialAccountingEngine.Application/FinancialAccountingEngine.Application.csproj src/FinancialAccountingEngine.Application/
COPY src/FinancialAccountingEngine.Infrastructure/FinancialAccountingEngine.Infrastructure.csproj src/FinancialAccountingEngine.Infrastructure/
COPY src/FinancialAccountingEngine.Api/FinancialAccountingEngine.Api.csproj src/FinancialAccountingEngine.Api/
RUN dotnet restore src/FinancialAccountingEngine.Api/FinancialAccountingEngine.Api.csproj

# Copy the rest of the source and publish.
COPY src/ src/
RUN dotnet publish src/FinancialAccountingEngine.Api/FinancialAccountingEngine.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "FinancialAccountingEngine.Api.dll"]
