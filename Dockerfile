# ── Build stage ──
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj first for layer caching
COPY src/ProposalGenerator.Web/ProposalGenerator.Web.csproj src/ProposalGenerator.Web/
RUN dotnet restore src/ProposalGenerator.Web/ProposalGenerator.Web.csproj

# Copy everything else and publish
COPY . .
WORKDIR /src/src/ProposalGenerator.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime stage ──
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Install libs required by Aspose.Words for PDF rendering on Linux
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        libgdiplus \
        libc6-dev \
        libfontconfig1 \
        libfreetype6 \
        fonts-dejavu-core \
        fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Create data directories (will be overridden by volume mounts)
RUN mkdir -p /app/data /app/data/templates /app/data/price-import /app/data/storage

# Copy sample prices into the image as a fallback
COPY price-import/ /app/data/price-import/

# Expose port
EXPOSE 8080

# Environment variables – overridable at runtime
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection="" \
    Authentication__Enabled=false \
    BlobStorage__UseLocalFileSystem=true \
    BlobStorage__LocalBasePath=/app/data/storage \
    PriceImport__FolderPath=/app/data/price-import \
    Templates__FolderPath=/app/data/templates

ENTRYPOINT ["dotnet", "ProposalGenerator.Web.dll"]
