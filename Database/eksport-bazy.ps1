# =====================================================================
#  Eksport lokalnej bazy (struktura + dane) do pliku .dump
#  Uruchom w PowerShellu z katalogu głównego repozytorium:
#      .\Database\eksport-bazy.ps1
#  Skrypt zapyta o hasło użytkownika postgres.
#  Plik .dump jest w .gitignore – NIE wrzucaj go do repozytorium.
# =====================================================================

param(
    [string]$PgBin    = "C:\Program Files\PostgreSQL\18\bin",
    [int]   $Port     = 5433,
    [string]$Database = "herb-recognition-db",
    [string]$User     = "postgres"
)

$pgDump = Join-Path $PgBin "pg_dump.exe"
if (-not (Test-Path $pgDump)) {
    Write-Error "Nie znaleziono $pgDump. Podaj ścieżkę: .\Database\eksport-bazy.ps1 -PgBin 'C:\...\bin'"
    exit 1
}

$stamp = Get-Date -Format "yyyy-MM-dd_HH-mm"
$out   = Join-Path $PSScriptRoot "herb-recognition_$stamp.dump"

# format custom: skompresowany, przywracany przez pg_restore
# --no-owner / --no-acl: na serwerze baza ma innego właściciela (użytkownik "herb")
& $pgDump --host=localhost --port=$Port --username=$User --dbname=$Database `
          --format=custom --no-owner --no-acl --file=$out

if ($LASTEXITCODE -ne 0) {
    Write-Error "pg_dump zakończył się błędem."
    exit 1
}

$size = [math]::Round((Get-Item $out).Length / 1KB, 1)
Write-Host "Gotowe: $out ($size KB)"
