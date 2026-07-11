---
kind: configuration_system
name: 插件配置系统：JSON 文件持久化与属性变更自动同步
category: configuration_system
scope:
    - '**'
source_files:
    - Models/AgentIslandSettings.cs
    - Models/AiTextComponentSettings.cs
    - Models/RunAcpActionSettings.cs
    - Models/AgentIslandJsonContext.cs
    - Views/SettingsPages/AcpSettingsPage.axaml.cs
    - Views/SettingsPages/AiTextSettingsPage.axaml.cs
---

本仓库采用基于 JSON 文件的轻量级配置系统，通过 C# 属性模型 + System.Text.Json 序列化实现配置的加载、持久化与运行时热更新。核心设计要点如下：

1. **配置模型定义**：所有可配置项以 POCO 类形式集中在 `Models/` 目录（如 `AgentIslandSettings.cs`、`AiTextComponentSettings.cs`、`RunAcpActionSettings.cs`），每个设置类对应一个独立的 JSON 配置文件。
2. **JSON 序列化上下文**：使用 `Models/AgentIslandJsonContext.cs` 中的 `[JsonSerializable]` 特性生成强类型序列化器，避免反射开销并支持 Avalonia 数据绑定。
3. **设置页驱动配置**：每个设置类在 `Views/SettingsPages/` 下提供对应的 Avalonia 设置页面（如 `AcpSettingsPage.axaml`、`AiTextSettingsPage.axaml`），通过 FluentAvalonia 的 `SettingsControl` 模式实现可视化编辑。
4. **自动同步机制**：设置类属性通常实现 `INotifyPropertyChanged`，配合 Avalonia 的数据绑定实现 UI 修改即时反映到内存对象；持久化由服务层在适当时机调用 `System.Text.Json.JsonSerializer.SerializeAsync` 写入磁盘。
5. **作用域划分**：全局插件级配置位于 `AgentIslandSettings`，组件级配置（如 AiText 组件）独立存储，动作级配置（如 RunAcpAction）各自隔离，便于 ClassIsland 插件宿主按需加载。
6. **无外部依赖**：不引入 Microsoft.Extensions.Configuration 等重型框架，仅依赖 .NET 内置 JSON 序列化，保持 ClassIsland 插件的轻量化要求。

开发者约定：新增配置项需同时更新对应 Settings 类、添加 JsonContext 注册、创建或扩展对应设置页面，并确保属性变更能触发持久化。