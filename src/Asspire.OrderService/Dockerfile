FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Asspire.OrderService/Asspire.OrderService.csproj", "src/Asspire.OrderService/"]
COPY ["src/Asspire.ServiceDefaults/Asspire.ServiceDefaults.csproj", "src/Asspire.ServiceDefaults/"]
RUN dotnet restore "src/Asspire.OrderService/Asspire.OrderService.csproj"
COPY . .
WORKDIR "/src/src/Asspire.OrderService"
RUN dotnet build "Asspire.OrderService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Asspire.OrderService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Asspire.OrderService.dll"]
