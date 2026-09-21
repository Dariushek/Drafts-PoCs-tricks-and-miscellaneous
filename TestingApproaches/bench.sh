#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RESULTS="$SCRIPT_DIR/bench-results"

declare -A PROJECTS=(
    [DbMockInMemory]="$SCRIPT_DIR/DbMockInMemory.Tests/DbMockInMemory.Tests.csproj"
    [DbSqlContainerInMemory]="$SCRIPT_DIR/DbSqlContainerInMemory.Tests/DbSqlContainerInMemory.Tests.csproj"
    [DbMongoContainerInMemory]="$SCRIPT_DIR/DbMongoContainerInMemory.Tests/DbMongoContainerInMemory.Tests.csproj"
)

run() {
    local runs="${1:-5}"

    for name in "${!PROJECTS[@]}"; do
        local csproj="${PROJECTS[$name]}"

        for i in $(seq 1 "$runs"); do
            local dir="$RESULTS/$name/run$i"
            mkdir -p "$dir"

            local start end
            start=$(date +%s.%N)
            dotnet test "$csproj" --no-build --no-restore \
                --logger "trx;LogFileName=result.trx" \
                --results-directory "$dir" \
                > "$dir/console.log" 2>&1 || true
            end=$(date +%s.%N)

            awk -v s="$start" -v e="$end" 'BEGIN { printf "%.3f\n", e - s }' > "$dir/wallclock.txt"
            echo "$name run $i: $(cat "$dir/wallclock.txt")s"
        done
    done
}

# extracts the raw value of every matching attribute node, one per line
xattr() {
    xmllint --xpath "$2" "$1" 2>/dev/null | sed -E 's/.*="([^"]*)"/\1/'
}

# converts TRX "HH:MM:SS.fffffff" duration strings (stdin) to seconds (stdout)
hms_to_seconds() {
    awk -F: '{ printf "%.7f\n", $1 * 3600 + $2 * 60 + $3 }'
}

# reads numbers (one per line) from stdin, prints n/mean/median/stdev/min/max
stats() {
    awk '
    { vals[NR] = $1; sum += $1 }
    END {
        n = NR
        if (n == 0) { print "no data"; exit }
        mean = sum / n
        asort(vals)
        median = (n % 2 == 1) ? vals[(n + 1) / 2] : (vals[n / 2] + vals[n / 2 + 1]) / 2
        for (i = 1; i <= n; i++) sqsum += (vals[i] - mean) ^ 2
        stdev = sqrt(sqsum / n)
        printf "n=%-3d mean=%7.3fs median=%7.3fs stdev=%6.3fs min=%7.3fs max=%7.3fs\n", n, mean, median, stdev, vals[1], vals[n]
    }'
}

test_phase_seconds() {
    local trx="$1"
    local start finish
    start=$(xmllint --xpath "string(//*[local-name()='Times']/@start)" "$trx" 2>/dev/null)
    finish=$(xmllint --xpath "string(//*[local-name()='Times']/@finish)" "$trx" 2>/dev/null)
    [[ -z "$start" || -z "$finish" ]] && return 0
    awk -v s="$(date -d "$start" +%s.%N)" -v f="$(date -d "$finish" +%s.%N)" 'BEGIN { printf "%.3f\n", f - s }'
}

report() {
    local per_test=false
    [[ "${1:-}" == "--per-test" ]] && per_test=true

    for project_dir in "$RESULTS"/*/; do
        [[ -d "$project_dir" ]] || continue
        local name
        name=$(basename "$project_dir")

        local wallclocks=() test_phases=() summed=()
        local pertest_file
        pertest_file=$(mktemp)

        for run_dir in "$project_dir"run*/; do
            local trx="$run_dir/result.trx"
            local wc="$run_dir/wallclock.txt"

            [[ -f "$wc" ]] && wallclocks+=("$(cat "$wc")")
            [[ -f "$trx" ]] || continue

            local phase
            phase=$(test_phase_seconds "$trx")
            [[ -n "$phase" ]] && test_phases+=("$phase")

            local run_sum
            run_sum=$(xattr "$trx" "//*[local-name()='UnitTestResult']/@duration" | hms_to_seconds | awk '{ s += $1 } END { printf "%.3f", s }')
            summed+=("$run_sum")

            paste -d'\t' \
                <(xattr "$trx" "//*[local-name()='UnitTestResult']/@testName") \
                <(xattr "$trx" "//*[local-name()='UnitTestResult']/@duration" | hms_to_seconds) \
                >> "$pertest_file"
        done

        echo
        echo "=== $name ==="
        printf '%s\n' "${wallclocks[@]}" | stats | sed 's/^/external wall clock (dotnet test process)     /'
        printf '%s\n' "${test_phases[@]}" | stats | sed 's/^/trx test-phase duration (excludes CLI boot)   /'
        printf '%s\n' "${summed[@]}" | stats | sed 's/^/sum of per-test durations (pure test work)    /'

        if $per_test; then
            echo "  slowest individual tests (median across runs):"
            awk -F'\t' '
            { durs[$1][++cnt[$1]] = $2 }
            END {
                for (name in durs) {
                    n = cnt[name]
                    asort(durs[name])
                    med = (n % 2 == 1) ? durs[name][(n + 1) / 2] : (durs[name][n / 2] + durs[name][n / 2 + 1]) / 2
                    printf "%f\t%s\n", med, name
                }
            }' "$pertest_file" | sort -rn | head -5 | awk -F'\t' '{ printf "    %-60s median=%7.1fms\n", $2, $1 * 1000 }'
        fi

        rm -f "$pertest_file"
    done
}

case "${1:-}" in
    run) shift; run "$@" ;;
    report) shift; report "$@" ;;
    *) run "${1:-5}"; report ;;
esac
