# =====================================================================
#  HerbRecognition_APIs
#  Kontekst budowania: folder HerbRecognition_APIs (tam, gdzie jest .csproj)
# =====================================================================

ARG DOTNET_VERSION=9.0

# ---------- etap 1: kompilacja ----------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# najpierw tylko .csproj – pakiety NuGet pobierają się ponownie
# tylko wtedy, gdy zmienią się zależności (szybsze kolejne wdrożenia)
COPY HerbRecognition_APIs.csproj ./
RUN dotnet restore HerbRecognition_APIs.csproj

COPY . .
# w folderze jest też plik .sln, więc wskazujemy projekt wprost
RUN dotnet publish HerbRecognition_APIs.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- etap 2: lekki obraz do uruchomienia ----------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production

# folder na zdjęcia roślin (trwały wolumen w docker-compose); właścicielem jest
# użytkownik "app", żeby aplikacja mogła zapisywać pliki
RUN mkdir -p /data/images && chown -R $APP_UID /data

COPY --from=build /app/publish .

# aplikacja nie działa jako root
USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "HerbRecognition_APIs.dll"]
