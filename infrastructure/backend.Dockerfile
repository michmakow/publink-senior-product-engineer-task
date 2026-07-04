FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Publink.AuditTimeline.sln ./
COPY src/backend/Publink.AuditTimeline.Api/Publink.AuditTimeline.Api.csproj src/backend/Publink.AuditTimeline.Api/
COPY src/backend/Publink.AuditTimeline.Application/Publink.AuditTimeline.Application.csproj src/backend/Publink.AuditTimeline.Application/
COPY src/backend/Publink.AuditTimeline.Domain/Publink.AuditTimeline.Domain.csproj src/backend/Publink.AuditTimeline.Domain/
COPY src/backend/Publink.AuditTimeline.Infrastructure/Publink.AuditTimeline.Infrastructure.csproj src/backend/Publink.AuditTimeline.Infrastructure/

RUN dotnet restore src/backend/Publink.AuditTimeline.Api/Publink.AuditTimeline.Api.csproj

COPY src/backend src/backend
RUN dotnet publish src/backend/Publink.AuditTimeline.Api/Publink.AuditTimeline.Api.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Publink.AuditTimeline.Api.dll"]
