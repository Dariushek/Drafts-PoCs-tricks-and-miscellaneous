#!/usr/bin/env bash
set -euo pipefail

CONTAINER_NAME="dbsqlcontainerinmemory-dev-mssql"
IMAGE="mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04"
HOST_PORT=14330
SA_PASSWORD='yourStrong(!)Password'

up() {
    if docker ps --filter "name=^${CONTAINER_NAME}\$" --format '{{.Names}}' | grep -q .; then
        echo "already running: $CONTAINER_NAME"
        return
    fi

    # Always recreate rather than `docker start` a stopped one: tmpfs mounts
    # don't survive a restart with the right permissions for the non-root
    # mssql user, so a stopped container would crash-loop on start.
    docker rm -f "$CONTAINER_NAME" > /dev/null 2>&1 || true

    docker run -d \
        --name "$CONTAINER_NAME" \
        -p "${HOST_PORT}:1433" \
        -e ACCEPT_EULA=Y \
        -e MSSQL_SA_PASSWORD="$SA_PASSWORD" \
        --tmpfs /var/opt/mssql/data \
        --tmpfs /var/opt/mssql/log \
        --tmpfs /var/opt/mssql/secrets \
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
# (dropping per-test or in bulk both proved slower/flakier than just leaving
# them — see SqlContainerSetup.cs). Recreating the container is the cheap,
# reliable way to reset: fresh tmpfs, done in seconds.
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
