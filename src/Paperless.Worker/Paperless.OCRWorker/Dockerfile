FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props global.json ./
COPY src/Paperless.Worker/ src/Paperless.Worker/
RUN dotnet publish src/Paperless.Worker/Paperless.OCRWorker/Paperless.OCRWorker.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Paperless.OCRWorker.dll"]
