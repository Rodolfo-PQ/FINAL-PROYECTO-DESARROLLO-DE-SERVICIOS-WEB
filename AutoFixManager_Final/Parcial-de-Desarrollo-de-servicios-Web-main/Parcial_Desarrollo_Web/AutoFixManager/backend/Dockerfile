# Etapa de compilación: el SDK restaura y publica la aplicación .NET.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY AutoFixManager.Api/AutoFixManager.Api.csproj AutoFixManager.Api/
RUN dotnet restore AutoFixManager.Api/AutoFixManager.Api.csproj
COPY AutoFixManager.Api/ AutoFixManager.Api/
WORKDIR /src/AutoFixManager.Api
RUN dotnet publish AutoFixManager.Api.csproj -c Release -o /app/publish --no-restore

# Etapa final: contiene solo el runtime y los archivos publicados.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
# ASP.NET escucha en todas las interfaces del contenedor por el puerto 8080.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
# The named Docker volume mounted here must be writable by the non-root runtime user.
RUN mkdir -p /app/backups && chown "$APP_UID" /app/backups
# La imagen oficial define APP_UID para ejecutar sin privilegios administrativos.
USER $APP_UID
ENTRYPOINT ["dotnet", "AutoFixManager.Api.dll"]
