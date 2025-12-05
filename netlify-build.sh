#!/usr/bin/env bash
set -e

# Restore NuGet packages
dotnet restore

# Install Uno Platform WASM workload (required for net8.0-wasm)
 dotnet workload install uno-wasm

# Publish the WebAssembly project
 dotnet publish MemoryMatch/MemoryMatch.csproj -c Release -p:TargetFramework=net8.0-wasm -o ./publish
