FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/Asspire.Gateway/Asspire.Gateway.csproj", "src/Asspire.Gateway/"]
COPY ["src/Asspire.ServiceDefaults/Asspire.ServiceDefaults.csproj", "src/Asspire.ServiceDefaults/"]

# Restore dependencies
RUN dotnet restore "src/Asspire.Gateway/Asspire.Gateway.csproj"

# Copy all source code
COPY . .

# Build the project
WORKDIR "/src/src/Asspire.Gateway"
RUN dotnet build "Asspire.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the project
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Asspire.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Asspire.Gateway.dll"]
