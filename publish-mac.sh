#!/usr/bin/env bash
set -euo pipefail

echo "Publishing for macOS (osx-arm64)..."
dotnet publish FantaniaApp/Fantania.csproj -c Release -r osx-arm64 --self-contained true -o ./Publish/osx-arm64
echo "Done."
