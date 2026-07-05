This repository uses the standard .NET SDK-style project system (Microsoft.NET.Sdk) for dependency management. There is no global `NuGet.config` or `global.json`; all package resolution relies on the default nuget.org feed and MSBuild's built-in restore behavior.

**Top-level project (`AgentIsland.csproj`)**
- Targets `net8.0-windows` and declares four direct NuGet packages:
  - `ClassIsland.PluginSdk` — version pinned via a `<PropertyGroup>` variable `ClassIslandPluginSdkVersion=1.7.106.2-dev-v2`
  - `DotNetCampus.ModelContextProtocol` 0.1.0-alpha.40
  - `AgentClientProtocol` 0.1.5
  - `Sentry` 5.14.1
- The project also contains explicit `<None Include="$(NuGetPackageRoot)..." CopyToOutputDirectory="PreserveNewest">` items that copy selected transitive DLLs (`DotNetCampus.ModelContextProtocol.dll`, `System.Text.Json.dll`, `System.Text.Encodings.Web.dll`, `System.IO.Pipelines.dll`, `Sentry.dll`) into the plugin output folder alongside the main assembly. This is an unusual workaround to ensure these runtime dependencies ship with the ClassIsland plugin bundle, since the host does not automatically resolve them from the NuGet cache.
- A `<Compile Remove="reference\**" />` item group excludes the vendored `reference/IslandMQ` subproject from compilation; it is treated as source-only code rather than a referenced project.

**Vendored subproject (`reference/IslandMQ/`)**
- A self-contained C# library included as source under `reference/`. It has its own `.csproj`, `.sln`, and a full `packages.lock.json` (version 1 lockfile), pinning every direct and transitive dependency to exact versions with content hashes. Direct dependencies include `ClassIsland.PluginSdk [2.0.0.1,)`, `JsonSchema.Net [9.2.1,)`, `NetMQ [4.0.4.2,)`, and `Sisk.HttpServer [1.6.2,)`, plus a large set of Avalonia UI packages.
- The top-level project compiles this code in-place by excluding it from the main build but keeping it available for reference. No `<ProjectReference>` is used — the files are simply part of the repo tree.
- A separate `renovate.json` exists inside `reference/IslandMQ/`, indicating automated PR-based update scanning for that subproject's lockfile.

**No lockfile at the root**
The root `AgentIsland.csproj` does not generate or commit a `packages.lock.json`; only the vendored `reference/IslandMQ/` directory maintains one. This means the top-level project allows flexible transitive resolution while the vendored subproject enforces deterministic builds.

**Conventions observed**
- Plugin SDK version is centralized in a single MSBuild property so it can be bumped in one place.
- Runtime DLLs needed by the ClassIsland host are explicitly copied out of the NuGet cache via `$(NuGetPackageRoot)` paths rather than relying on implicit package assets.
- Vendored third-party code lives under `reference/` and is excluded from the main build via ItemGroup removes.