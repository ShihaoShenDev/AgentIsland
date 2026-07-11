---
kind: dependency_management
name: NuGet 包管理与锁定文件策略
category: dependency_management
scope:
    - '**'
source_files:
    - AgentIsland.csproj
    - reference/IslandMQ/packages.lock.json
    - reference/IslandMQ/IslandMQ.csproj
---

本仓库采用 .NET SDK 风格的 NuGet 包管理，通过 `<PackageReference>` 在 `.csproj` 中声明依赖，并使用 `packages.lock.json` 锁定解析后的版本。两个项目采用了略有不同的锁定策略：

- **AgentIsland（主插件）**：位于根目录的 `AgentIsland.csproj` 直接引用了 `ClassIsland.PluginSdk`、`DotNetCampus.ModelContextProtocol`、`AgentClientProtocol`、`Sentry` 等 NuGet 包，并通过 `<None Include="$(NuGetPackageRoot)..." CopyToOutputDirectory="PreserveNewest">` 显式将运行时 DLL 复制到输出目录，以适配 ClassIsland 插件宿主对程序集位置的期望。
- **reference/IslandMQ（子模块）**：该子项目启用了 `RestorePackagesWithLockFile=true`，并在其目录下维护了完整的 `packages.lock.json`，用于锁定所有直接和传递依赖的版本与哈希，确保可重复构建。

关键约定：
1. 所有第三方依赖均通过 NuGet 引入，未使用 vendor 或 git submodule 方式；`reference/IslandMQ` 虽为子目录但作为独立项目存在，不通过项目引用被主工程包含（主工程的 `<Compile Remove="reference\**" />` 排除了它）。
2. 版本号在 `.csproj` 中以硬编码形式声明，未使用中央版本管理文件或 `Directory.Packages.props`。
3. 未配置私有 NuGet 源或 `nuget.config`，默认使用 nuget.org。
4. 无全局 `global.json` 指定 SDK 版本，仅通过 `<TargetFramework>net8.0-windows</TargetFramework>` 约束目标框架。
5. 主项目未启用 `packages.lock.json`，而 IslandMQ 子项目启用了锁定，两者策略不一致。