FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Asspire.ProductService/Asspire.ProductService.csproj", "src/Asspire.ProductService/"]
COPY ["src/Asspire.ServiceDefaults/Asspire.ServiceDefaults.csproj", "src/Asspire.ServiceDefaults/"]
RUN dotnet restore "src/Asspire.ProductService/Asspire.ProductService.csproj"
COPY . .
WORKDIR "/src/src/Asspire.ProductService"
RUN dotnet build "Asspire.ProductService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Asspire.ProductService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Asspire.ProductService.dll"]
