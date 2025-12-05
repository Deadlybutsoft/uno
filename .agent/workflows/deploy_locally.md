---
description: Deploy MemoryMatch Uno Platform app locally (WebAssembly)
---
## Prerequisites
1. **.NET SDK 8+** (or the version used by the solution).  
2. **Uno.Check** – run `dotnet tool install -g Uno.Check` if you haven’t already.  
3. A recent browser (Chrome/Edge/Firefox) for the WASM host.

## Steps

1. **Restore NuGet packages**  
   ```bash
   // turbo
   dotnet restore /Users/suvog/TerraNova_uno/uno-srt/MemoryMatch/MemoryMatch.csproj
   ```

2. **Build the WebAssembly head** (Debug build – fast iteration)  
   ```bash
   // turbo
   dotnet build /Users/suvog/TerraNova_uno/uno-srt/MemoryMatch/MemoryMatch.csproj \
       -c Debug -f net10.0-browserwasm
   ```

3. **Run the app** – this launches a local dev server and opens the browser automatically  
   ```bash
   // turbo
   dotnet run -p /Users/suvog/TerraNova_uno/uno-srt/MemoryMatch/MemoryMatch.csproj \
       -c Debug -f net10.0-browserwasm
   ```

   > **What happens?**  
   - The Uno Platform host (`Program.cs`) starts a lightweight Kestrel server.  
   - Your default browser opens `http://localhost:5000` (or the URL printed in the console).  
   - The app runs as a WebAssembly SPA.

4. **Optional – Production build** (Release + minified assets)  
   ```bash
   // turbo
   dotnet publish -p /Users/suvog/TerraNova_uno/uno-srt/MemoryMatch/MemoryMatch.csproj \
       -c Release -f net10.0-browserwasm -o ./publish
   ```

   You can then serve the `./publish` folder with any static‑file server, e.g.:

   ```bash
   // turbo
   npx -y serve ./publish
   ```

## Verification

- After step 3 the browser should display the **MemoryMatch** game UI.  
- Open the browser console (F12) – there should be **no errors** and the page title reads “MemoryMatch”.

## Clean‑up

When you’re done, stop the running process with `Ctrl+C` in the terminal.
