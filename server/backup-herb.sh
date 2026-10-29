#!/usr/bin/env bash
# =====================================================================
#  Kopia zapasowa bazy i zdjęć HerbRecognition (uruchamiana codziennie przez cron)
#  Instalacja: patrz docs/WDROZENIE.md, etap 6.
# =====================================================================
set -euo pipefail

BACKUP_DIR=/var/backups/herb
KEEP_DAYS=14
STAMP=$(date +%F_%H-%M)

mkdir -p "$BACKUP_DIR"

# ---------- baza ----------
DB_CONTAINER=$(docker ps --format '{{.Names}}' | grep -E '(^|-)db(-|$)' | head -n1 || true)

if [ -z "$DB_CONTAINER" ]; then
    echo "$(date) BŁĄD: nie znaleziono działającego kontenera bazy" >&2
    exit 1
fi

docker exec "$DB_CONTAINER" sh -c 'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" --format=custom' \
    > "$BACKUP_DIR/db_$STAMP.dump"

# ---------- zdjęcia ----------
IMAGES_VOLUME=$(docker volume ls --format '{{.Name}}' | grep -E '(^|_)images$' | head -n1 || true)

if [ -n "$IMAGES_VOLUME" ]; then
    docker run --rm \
        -v "$IMAGES_VOLUME":/data:ro \
        -v "$BACKUP_DIR":/backup \
        alpine tar czf "/backup/images_$STAMP.tar.gz" -C /data .
fi

# ---------- usuwanie starych kopii ----------
find "$BACKUP_DIR" -type f -mtime +"$KEEP_DAYS" -delete

echo "$(date) OK: kopia $STAMP zapisana w $BACKUP_DIR"
