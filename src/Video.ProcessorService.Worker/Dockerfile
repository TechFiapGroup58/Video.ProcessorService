FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

COPY Video.ProcessorService.sln .
COPY src/Video.ProcessorService.Core/Video.ProcessorService.Core.csproj \
     src/Video.ProcessorService.Core/
COPY src/Video.ProcessorService.DataSource/Video.ProcessorService.DataSource.csproj \
     src/Video.ProcessorService.DataSource/
COPY src/Video.ProcessorService.Messaging/Video.ProcessorService.Messaging.csproj \
     src/Video.ProcessorService.Messaging/
COPY src/Video.ProcessorService.Worker/Video.ProcessorService.Worker.csproj \
     src/Video.ProcessorService.Worker/

RUN dotnet restore Video.ProcessorService.sln
COPY src/ src/
RUN dotnet publish src/Video.ProcessorService.Worker/Video.ProcessorService.Worker.csproj \
      -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends ffmpeg \
    && rm -rf /var/lib/apt/lists/*
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Video.ProcessorService.Worker.dll"]
