# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY *.sln .
COPY APISegaAI/APISegaAI.csproj APISegaAI/
COPY APISegaAI.Domain/APISegaAI.Domain.csproj APISegaAI.Domain/
COPY APISegaAI.Service/APISegaAI.Service.csproj APISegaAI.Service/
COPY APISegaAI.DAL/APISegaAI.DAL.csproj APISegaAI.DAL/
RUN dotnet restore
COPY . .
RUN dotnet publish APISegaAI/APISegaAI.csproj -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
# Создаём папку Certificates и копируем сертификат из правильного пути
RUN mkdir -p Certificates
COPY --from=build /app/APISegaAI/Certificates/cbr-S02D-CBRDC01-CA.cer Certificates/
RUN ls -la Certificates/  # Для отладки

# Устанавливаем ca-certificates и добавляем сертификат в системное хранилище
RUN apt-get update && apt-get install -y ca-certificates \
    && cp Certificates/cbr-S02D-CBRDC01-CA.cer /usr/local/share/ca-certificates/ \
    && update-ca-certificates

EXPOSE 80
ENTRYPOINT ["dotnet", "APISegaAI.dll"]