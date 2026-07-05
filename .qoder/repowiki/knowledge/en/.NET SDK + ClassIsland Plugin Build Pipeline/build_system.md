The project is a single-class-library .NET 8 plugin for the ClassIsland desktop host. The build system revolves around the standard Microsoft.NET.Sdk with a small set of PowerShell helper scripts and a GitHub Actions workflow that produces the distributable `.cipx` package.

**Build targets and framework**
- `AgentIsland.csproj` targets `net8.0-windows` (Windows-only).
- It references `ClassIsland.PluginSdk` via a version property `ClassIslandPluginSdkVersion=1.7.106.2-dev-v2`, plus runtime NuGet packages (`DotNetCampus.ModelContextProtocol`, `AgentClientProtocol`, `Sentry`).
- Several runtime DLLs (`DotNetCampus.ModelContextProtocol.dll`, `System.Text.Json.dll`, `System.Text.Encodings.Web.dll`, `System.IO.Pipelines.dll`, `Sentry.dll`) are copied into the output directory as `None` items so they ship alongside the plugin assembly.
- `manifest.yml` (id, name, version, apiVersion, supportedOSPlatforms) is copied to the output; it is consumed by the ClassIsland packaging pipeline.

**Local development scripts (PowerShell)**
- `build-debug.ps1`: kills any running `ClassIsland.Desktop.exe`, runs `dotnet build`, then launches the ClassIsland debug binary from `%ClassIsland_DebugBinaryDirectory%` with `-epp <bin/Debug/net8.0-windows>` to hot-load the plugin.
- `build-release.ps1`: same flow but uses `dotnet publish -c Release` and points at `<bin/Release/net8.0-windows/publish>`.
- `create-cipx.ps1`: invokes `dotnet publish -c Release -p:CreateCipx=true`; this MSBuild property is provided by the ClassIsland Plugin SDK and produces the final `cipx/AgentIsland.cipx` artifact.

**CI (GitHub Actions)**
- `.github/workflows/cipx.yml` runs on `windows-latest`, installs .NET 8.0.x, checks out the repo, and executes `dotnet publish -c Release -p:CreateCipx=true`. The resulting `cipx/` folder is uploaded as an artifact named "CIPX Package".

**Packaging & distribution**
- The `.cipx` file is the ClassIsland plugin archive produced by the SDK's `CreateCipx` target. The CI uploads it as a build artifact; local publishing follows the same command.
- Versioning is declared in `manifest.yml` (`version: 1.0.0.0`) and is separate from the SDK version property in the `.csproj`.
- Output layout mirrors what ClassIsland expects: `AgentIsland.dll` plus its runtime dependencies and `manifest.yml` under the publish folder, which the SDK then zips into `cipx/AgentIsland.cipx`.

**Conventions developers should follow**
- Use `build-debug.ps1` / `build-release.ps1` / `create-cipx.ps1` instead of invoking `dotnet` directly, so the correct ClassIsland host process is launched with the right `-epp` path.
- Keep `manifest.yml` in sync with the intended plugin id/name/version; the SDK reads it during CIPX creation.
- Do not add extra native/runtime assemblies without also adding them as `None CopyToOutputDirectory="PreserveNewest"` items in the `.csproj`, otherwise they will be missing from the published plugin.