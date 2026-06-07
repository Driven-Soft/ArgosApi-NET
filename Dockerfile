# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia só os .csproj primeiro para aproveitar o cache do restore
COPY Argos.sln ./
COPY Argos.Domain/Argos.Domain.csproj            Argos.Domain/
COPY Argos.Application/Argos.Application.csproj   Argos.Application/
COPY Argos.Infrastructure/Argos.Infrastructure.csproj Argos.Infrastructure/
COPY Argos.Api/Argos.Api.csproj                   Argos.Api/
RUN dotnet restore Argos.Api/Argos.Api.csproj

# Copia o resto do código e publica
COPY . .
RUN dotnet publish Argos.Api/Argos.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# O Render injeta a porta em $PORT (default 10000). O .NET não lê PORT sozinho,
# então amarramos o Kestrel a ela em tempo de execução (fallback 8080 para rodar local).
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet Argos.Api.dll"]
