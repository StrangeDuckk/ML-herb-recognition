# =====================================================================
#  HerbRecognition_Web
#  Kontekst budowania: folder HerbRecognition_Web (tam, gdzie jest .csproj)
# =====================================================================

ARG DOTNET_VERSION=9.0

# ---------- etap 1: kompilacja ----------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# najpierw tylko .csproj – pakiety NuGet pobierają się ponownie
# tylko wtedy, gdy zmienią się zależności (szybsze kolejne wdrożenia)
COPY HerbRecognition_Web.csproj ./
RUN dotnet restore HerbRecognition_Web.csproj

COPY . .
# w folderze jest też plik .sln, więc wskazujemy projekt wprost
RUN dotnet publish HerbRecognition_Web.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- etap 2: lekki obraz do uruchomienia ----------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production

# /data/keys  – klucze Data Protection (trwały wolumen, żeby po wdrożeniu nie
#               unieważniały się formularze i sesje użytkowników)
# /data/images – zdjęcia roślin (wspólny wolumen z API, tylko do odczytu)
RUN mkdir -p /data/keys /data/images && chown -R $APP_UID /data

COPY --from=build /app/publish .

# aplikacja nie działa jako root
USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "HerbRecognition_Web.dll"]
