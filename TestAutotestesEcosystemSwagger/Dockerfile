# Используем Windows-образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-ltsc2022 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-ltsc2022 AS build
WORKDIR /src
COPY ["TestAutotestesEcosystemSwagger.csproj", "."]
RUN dotnet restore "TestAutotestesEcosystemSwagger.csproj"
COPY . .
RUN dotnet build "TestAutotestesEcosystemSwagger.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestAutotestesEcosystemSwagger.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestAutotestesEcosystemSwagger.dll"]