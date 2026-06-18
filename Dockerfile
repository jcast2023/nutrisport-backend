# Etapa 1: Base de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Etapa 2: Compilación del código
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NutricionMacros.API.csproj", "."]
RUN dotnet restore "./NutricionMacros.API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "NutricionMacros.API.csproj" -c Release -o /app/build

# Etapa 3: Publicación
FROM build AS publish
RUN dotnet publish "NutricionMacros.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 4: Imagen final lista para correr
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NutricionMacros.API.dll"]