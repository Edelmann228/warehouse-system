# ── Этап 1: сборка ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Сначала копируем только .csproj и восстанавливаем зависимости
# (слой кэшируется Docker'ом пока .csproj не изменится)
COPY ["WarehouseApi/WarehouseApi.csproj", "WarehouseApi/"]
RUN dotnet restore "WarehouseApi/WarehouseApi.csproj"
# Теперь копируем весь исходный код
COPY . .
WORKDIR "/src/WarehouseApi"
RUN dotnet build -c Release -o /app/build
 
# ── Этап 2: публикация ────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish
 
# ── Этап 3: финальный образ (без SDK, только runtime) ─────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "WarehouseApi.dll"]
