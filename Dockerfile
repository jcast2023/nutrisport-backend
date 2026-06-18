FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["NutricionMacros.API/NutricionMacros.API.csproj", "NutricionMacros.API/"]
RUN dotnet restore "NutricionMacros.API/NutricionMacros.API.csproj"

# Copiamos todo el resto del código
COPY . .
WORKDIR "/src/NutricionMacros.API"
RUN dotnet build "NutricionMacros.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NutricionMacros.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NutricionMacros.API.dll"]