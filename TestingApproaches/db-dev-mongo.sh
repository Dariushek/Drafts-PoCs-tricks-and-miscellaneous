#!/usr/bin/env bash
set -euo pipefail

CONTAINER_NAME="dbmongocontainerinmemory-dev-mongo"
IMAGE="mongo:6.0"
HOST_PORT=14340
ROOT_USERNAME="root"
ROOT_PASSWORD='yourStrongPassword'

up() {
    if docker ps --filter "name=^${CONTAINER_NAME}\$" --format '{{.Names}}' | grep -q .; then
        echo "already running: $CONTAINER_NAME"
        return
    fi

    # Always recreate rather than `docker start` a stopped one - same reason
    # as db-dev.sh: a tmpfs-backed container isn't guaranteed to come back
    # correctly on restart, so don't rely on it.
    docker rm -f "$CONTAINER_NAME" > /dev/null 2>&1 || true

    docker run -d \
        --name "$CONTAINER_NAME" \
        -p "${HOST_PORT}:27017" \
        -e MONGO_INITDB_ROOT_USERNAME="$ROOT_USERNAME" \
        -e MONGO_INITDB_ROOT_PASSWORD="$ROOT_PASSWORD" \
        --tmpfs /data/db \
        "$IMAGE" > /dev/null

    echo "created and started: $CONTAINER_NAME (localhost:${HOST_PORT})"
}

down() {
    docker rm -f "$CONTAINER_NAME" 2>/dev/null && echo "removed: $CONTAINER_NAME" || echo "not running: $CONTAINER_NAME"
}

status() {
    docker ps -a --filter "name=^${CONTAINER_NAME}\$" --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'
}

# Test databases accumulate on this container since tests don't drop them
# (see db-dev.sh's clean for the same reasoning on the SQL side). Recreating
# the container is the cheap, reliable way to reset.
clean() {
    down
    up
}

case "${1:-}" in
    up) up ;;
    down) down ;;
    status) status ;;
    clean) clean ;;
    *) echo "usage: $0 {up|down|status|clean}" >&2; exit 1 ;;
esac
