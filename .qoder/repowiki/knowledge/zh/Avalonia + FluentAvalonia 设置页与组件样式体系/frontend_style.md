本仓库为 ClassIsland 桌面插件，UI 基于 Avalonia XAML 构建，并采用 FluentAvalonia 控件库实现 Windows 11 Fluent Design 风格。前端样式体系的核心特征如下：

- **框架与主题**：使用 `https://github.com/avaloniaui` 命名空间声明 Avalonia 基础控件，通过 `xmlns:fa="clr-namespace:FluentAvalonia.UI.Controls;assembly=FluentAvalonia"` 引入 FluentAvalonia 的 `SettingsExpander`、`ComboBox` 等高级控件；所有设置页面继承自宿主 SDK 提供的 `ci:SettingsPageBase`，组件继承 `ci:ComponentBase`，由 ClassIsland 宿主统一注入主题与资源。
- **样式组织方式**：未使用独立 CSS/SCSS 文件，所有视觉样式内联于 `.axaml` 中，包括 `Background="#20FF9800"`、`CornerRadius="4"`、`Padding="12,8"`、`Opacity="0.4"` 等硬编码值，以及 `Classes="settings-container animated-intro"` 这类轻量类名用于分组。
- **设计令牌**：颜色以十六进制常量直接写入（如 `#FF9800`、`#14007ACC`、`#FFA726`），未见集中式 `ResourceDictionary` 或主题切换机制；字体大小、间距通过 `FontSize="16"`、`Spacing="8"` 等属性散布在各控件上。
- **响应式策略**：布局依赖 Avalonia 原生容器（`Grid`、`StackPanel`、`ScrollViewer`）的自适应能力，未引入媒体查询或断点系统；设计时尺寸通过 `d:DesignWidth="800" d:DesignHeight="600"` 标注。
- **图标与视觉元素**：使用 `ci:IconText`、`ci:FluentIcon`、`ci:FluentIconSource` 配合 Unicode Glyph 编号显示图标，前景色同样以内联 `Foreground="#FFA726"` 形式指定。

开发者应遵循的约定：
1. 新增设置页需继承 `ci:SettingsPageBase` 并在根节点声明 `ci` 命名空间。
2. 自定义组件需继承 `ci:ComponentBase<TSettings>` 并通过 `x:TypeArguments` 绑定设置模型类型。
3. 优先复用 FluentAvalonia 控件（`fa:SettingsExpander` 等）以保持与宿主一致的外观。
4. 避免在项目中新增独立 CSS/SCSS 文件——样式应保持在 AXAML 内联，或通过宿主提供的资源字典扩展。
5. 颜色与字号尽量从宿主主题获取，减少硬编码十六进制值以提升可维护性。