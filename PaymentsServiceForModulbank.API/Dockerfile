FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем файлы проектов
COPY PaymentsServiceForModulbank.API/*.csproj ./PaymentsServiceForModulbank.API/
COPY PaymentsServiceForModulebank.Core/*.csproj ./PaymentsServiceForModulebank.Core/
COPY PaymentsServiceForModulebank.Sqlite/*.csproj ./PaymentsServiceForModulebank.Sqlite/

# Восстанавливаем зависимости
RUN dotnet restore ./PaymentsServiceForModulbank.API/PaymentsServiceForModulbank.API.csproj

# Копируем весь исходный код
COPY . .

# Сборка
WORKDIR /src/PaymentsServiceForModulbank.API
RUN dotnet build PaymentsServiceForModulbank.API.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish PaymentsServiceForModulbank.API.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PaymentsServiceForModulbank.API.dll"]