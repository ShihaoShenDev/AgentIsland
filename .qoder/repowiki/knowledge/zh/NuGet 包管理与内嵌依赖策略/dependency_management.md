本仓库采用 .NET 8 + NuGet 的包管理方式，核心依赖集中在根项目 `AgentIsland.csproj` 中，并通过 `reference/IslandMQ` 子目录以源码形式内嵌另一个 ClassIsland 插件作为运行时依赖。具体策略如下：

1. **包声明与版本约束**
   - 所有第三方包通过 `<PackageReference>` 在 `AgentIsland.csproj` 中显式声明，包括 `ClassIsland.PluginSdk`（使用属性 `$(ClassIslandPluginSdkVersion)` 集中控制）、`DotNetCampus.ModelContextProtocol`、`AgentClientProtocol`、`Sentry`。
   - 未使用中央包管理（无 `Directory.Packages.props`、`global.json`），每个包版本号直接写在 csproj 中。

2. **锁定文件**
   - 主项目未包含 `packages.lock.json`；但内嵌的 `reference/IslandMQ` 子项目维护了完整的 `packages.lock.json`，用于锁定其自身及 Avalonia 生态等大量传递依赖的版本与 contentHash，确保可重复构建。

3. **私有/本地依赖处理**
   - `reference/IslandMQ` 作为源码子模块被直接纳入仓库，并在主项目的 ItemGroup 中使用 `<Compile Remove="reference\**" />` 等规则排除编译，避免与主项目冲突。该子项目通过自身的 `.csproj` 和 `packages.lock.json` 独立管理依赖。
   - 主项目通过 `ClassIsland.PluginSdk` 间接引用 ClassIsland 运行时，而非直接耦合源码。

4. **输出产物打包策略**
   - 通过多个 `<None Include="$(NuGetPackageRoot)..." CopyToOutputDirectory="PreserveNewest" Link="...">` 条目，将 `DotNetCampus.ModelContextProtocol.dll`、`System.Text.Json.dll`、`System.IO.Pipelines.dll`、`Sentry.dll` 等关键 DLL 复制到输出目录并重命名链接，以便 ClassIsland 宿主能正确发现这些运行时依赖。
   - 构建脚本 `build-debug.ps1`、`build-release.ps1`、`create-cipx.ps1` 负责最终打包为 cipx 插件包。

5. **开发者约定**
   - 新增 NuGet 包应在 `AgentIsland.csproj` 的 `<ItemGroup>` 中添加 `<PackageReference>`，并评估是否需要将其 DLL 加入 `<None Include="...">` 列表以复制到输出。
   - 若引入新的源码级子项目，应遵循 `reference/IslandMQ` 的模式：放在 `reference/` 下、提供独立 `.csproj` 与 `packages.lock.json`、在主项目中用 `<Compile Remove="reference\**" />` 排除编译。
   - 保持 `ClassIslandPluginSdkVersion` 属性与 ClassIsland 宿主版本对齐，避免运行时不兼容。