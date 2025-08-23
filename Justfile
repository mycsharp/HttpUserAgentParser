# Justfile .NET - Benjamin Abt 2025 - https://benjamin-abt.com
# https://github.com/BenjaminAbt/templates/blob/main/justfile/dotnet

set shell := ["pwsh", "-c"]

# ===== Configurable defaults =====
CONFIG  := "Debug"
TFM     := "net10.0"
BENCH_PRJ := "perf/HttpUserAgentParser.Benchmarks/HttpUserAgentParser.Benchmarks.csproj"

# ===== Default / Help =====
default: help

help:
    # Overview:
    just --list
    # Usage:
    #   just build
    #   just test
    #   just bench

# ===== Basic .NET Workflows =====
restore:
    dotnet restore

build *ARGS:
    dotnet build --configuration "{{CONFIG}}" --nologo --verbosity minimal {{ARGS}}

rebuild *ARGS:
    dotnet build --configuration "{{CONFIG}}" --nologo --verbosity minimal --no-incremental {{ARGS}}

clean:
    dotnet clean --configuration "{{CONFIG}}" --nologo

run *ARGS:
    dotnet run --project --framework "{{TFM}}" --configuration "{{CONFIG}}" --no-launch-profile {{ARGS}}

# ===== Quality / Tests =====
format:
    dotnet format --verbosity minimal

format-check:
    dotnet format --verify-no-changes --verbosity minimal

test *ARGS:
    dotnet test --configuration "{{CONFIG}}" --framework "{{TFM}}" --nologo --verbosity minimal {{ARGS}}

test-cov:
    dotnet test --configuration "{{CONFIG}}" --framework "{{TFM}}" --nologo --verbosity minimal /p:CollectCoverage=true /p:CoverletOutputFormat="cobertura,lcov,opencover" /p:CoverletOutput="./TestResults/coverage/coverage"


test-filter QUERY:
    dotnet test --configuration "{{CONFIG}}" --framework "{{TFM}}" --nologo --verbosity minimal --filter "{{QUERY}}"

# ===== Packaging / Release =====
pack *ARGS:
    dotnet pack --configuration "{{CONFIG}}" --nologo --verbosity minimal -o "./artifacts/packages" {{ARGS}}

publish *ARGS:
    dotnet publish --configuration "{{CONFIG}}" --framework "{{TFM}}" --nologo --verbosity minimal -o "./artifacts/publish/{{TFM}}" {{ARGS}}

publish-sc RID *ARGS:
    dotnet publish --configuration "{{CONFIG}}" --framework "{{TFM}}" --runtime "{{RID}}" --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false --nologo --verbosity minimal -o "./artifacts/publish/{{TFM}}-{{RID}}" {{ARGS}}

# ===== Benchmarks =====
bench *ARGS:
    dotnet run --configuration Release --project "{{BENCH_PRJ}}" --framework "{{TFM}}" {{ARGS}}

# ===== Housekeeping =====
clean-artifacts:
    if (Test-Path "./artifacts") { Remove-Item "./artifacts" -Recurse -Force }

clean-all:
    just clean
    just clean-artifacts
    # Optionally: git clean -xdf

# ===== Combined Flows =====
fmt-build:
    just format
    just build

ci:
    just clean
    just restore
    just format-check
    just build
    just test-cov
