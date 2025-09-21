# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# В зависимости от операционной системы хост-компьютеров, которые будут выполнять сборку контейнеров или запускать их, может потребоваться изменить образ, указанный в инструкции FROM.
# Дополнительные сведения см. на странице https://aka.ms/containercompat

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809 AS base
WORKDIR /app
EXPOSE 8080


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:8.0-windowsservercore-ltsc2019 AS build
# Установить Visual Studio Build Tools, они необходимы для публикации AOT
# Примечание. Для использования Visual Studio Build Tools требуется действительная лицензия Visual Studio.
RUN curl -SL --output vs_buildtools.exe https://aka.ms/vs/17/release/vs_buildtools.exe
RUN vs_buildtools.exe --installPath C:\BuildTools --add Microsoft.VisualStudio.Component.VC.Tools.x86.x64 Microsoft.VisualStudio.Component.Windows10SDK.19041 --quiet --wait --norestart --nocache
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TestAutotestesEcosystemSwagger.csproj", "."]
RUN dotnet restore "./TestAutotestesEcosystemSwagger.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./TestAutotestesEcosystemSwagger.csproj" -c %BUILD_CONFIGURATION% -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TestAutotestesEcosystemSwagger.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=true

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM mcr.microsoft.com/dotnet/runtime:8.0-nanoserver-1809 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["TestAutotestesEcosystemSwagger.exe"]