# Используем официальный образ .NET
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем все проекты (важно!)
COPY PaymentsServiceForModulbankAPI/*.csproj ./PaymentsServiceForModulbankAPI/
COPY PaymentsServiceForModulebank.Core/*.csproj ./PaymentsServiceForModulebank.Core/
COPY PaymentsServiceForModulebank.Sqlite/*.csproj ./PaymentsServiceForModulebank.Sqlite/

# Восстанавливаем зависимости
RUN dotnet restore ./PaymentsServiceForModulbankAPI/PaymentsServiceForModulbankAPI.csproj

# Копируем весь код
COPY . .

# Собираем проект
WORKDIR /src/PaymentsServiceForModulbankAPI
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentsServiceForModulbankAPI.dll"]