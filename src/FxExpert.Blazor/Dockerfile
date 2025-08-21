# Use .NET 9 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install Python (required for WebAssembly compilation)
RUN apt-get update && apt-get install -y python3 python3-pip && \
    ln -s /usr/bin/python3 /usr/bin/python

# Install required workloads for Blazor WebAssembly
RUN dotnet workload install wasm-tools

# Copy csproj files and restore dependencies
COPY ["src/FxExpert.Blazor/FxExpert.Blazor/FxExpert.Blazor.csproj", "src/FxExpert.Blazor/FxExpert.Blazor/"]
COPY ["src/FxExpert.Blazor/FxExpert.Blazor.Client/FxExpert.Blazor.Client.csproj", "src/FxExpert.Blazor/FxExpert.Blazor.Client/"]
COPY ["shared-types/shared-types.csproj", "shared-types/"]

RUN dotnet restore "src/FxExpert.Blazor/FxExpert.Blazor/FxExpert.Blazor.csproj"

# Copy source code
COPY . .

# Build and publish the application
WORKDIR "/src/src/FxExpert.Blazor/FxExpert.Blazor"
RUN dotnet publish "FxExpert.Blazor.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use .NET 9 runtime for the final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN groupadd -r blazor && useradd --no-log-init -r -g blazor blazor

# Copy published application
COPY --from=build /app/publish .

# Set ownership and permissions
RUN chown -R blazor:blazor /app
USER blazor

# Expose port 8080 (non-privileged port)
EXPOSE 8080

# Configure ASP.NET Core to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "FxExpert.Blazor.dll"]