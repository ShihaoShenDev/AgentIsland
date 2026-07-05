本仓库作为 ClassIsland 的桌面插件，前端 UI 完全基于 Avalonia XAML 构建，并深度依赖宿主框架 ClassIsland 提供的核心控件库。整体风格遵循 Fluent Design（Windows 11），通过 FluentAvalonia 控件集实现一致的视觉体验。

## 技术栈与基础
- **UI 框架**：Avalonia UI（XAML）
- **控件库**：FluentAvalonia（`fa:` 命名空间），提供 `SettingsExpander`、`ComboBox` 等 Fluent 风格控件
- **宿主扩展**：ClassIsland 自定义 XAML 命名空间 `http://classisland.tech/schemas/xaml/core`（`ci:`），提供 `SettingsPageBase`、`ActionSettingsControlBase`、`ComponentBase`、`IconText`、`FluentIconSource` 等基类与装饰控件
- **图标系统**：使用 Fluent System Icons 的 Glyph 编码（如 `&#xEDE9;`、`&#xEF1D;`）

## 布局与结构约定
- 所有设置页面继承自 `ci:SettingsPageBase`，统一采用 `ScrollViewer > StackPanel` 的纵向滚动布局，根容器添加 `Classes="settings-container animated-intro"` 以复用宿主样式
- 每个设置项封装在 `fa:SettingsExpander` 中，Header/Description/Footer 三段式组织，Footer 内放置具体输入控件（ToggleSwitch、TextBox、NumericUpDown 等）
- 自定义组件（AI 文字）继承 `ci:ComponentBase<TSettings>`，通过泛型参数绑定对应 Settings 模型
- 动作配置控件继承 `ci:ActionSettingsControlBase<TSettings>`，支持在 ClassIsland 自动化流程中嵌入

## 样式与主题策略
- **无独立 CSS/SCSS 文件**：所有样式直接内联于 XAML，通过 `Background="#20FF9800"`、`CornerRadius="4"`、`Padding="12,8"` 等属性定义
- **颜色语义化**：使用半透明背景色表达状态——`#20FF9800`（警告/不可用）、`#14007ACC`（信息块）、`#204CAF50`（成功提示）
- **动态资源**：部分文本和背景引用宿主主题资源，如 `{DynamicResource SystemFillColorSubtleBackgroundBrush}`、`{DynamicResource SystemControlForegroundBaseMediumBrush}`，确保跟随系统深色/浅色模式
- **字体层级**：标题使用 `FontSize="16" FontWeight="SemiBold"`，正文默认大小，辅助说明使用 `Opacity="0.6"` / `Opacity="0.5"` 降低视觉权重

## 设计决策
- 不维护独立样式表，将样式与布局耦合在 XAML 中，减少跨文件同步成本
- 通过 ClassIsland 的 `ci:` 命名空间抽象出通用基类，使插件 UI 与宿主保持一致的外观和行为
- 对未开放功能使用统一的“不可用”横幅（橙色背景 + 警告图标 + 禁用面板）传达状态
- 响应式通过 Avalonia 内置机制（如 `MaxWidth="360"`、`TextTrimming="CharacterEllipsis"`）处理，而非媒体查询