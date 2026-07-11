---
kind: frontend_style
name: Avalonia + FluentAvalonia XAML Styling
category: frontend_style
scope:
    - '**'
source_files:
    - AgentIsland.csproj
    - Components/AiTextComponent.axaml
    - Components/AiTextComponent.axaml.cs
    - Views/SettingsPages/AiTextSettingsPage.axaml
    - Views/SettingsPages/McpSettingsPage.axaml
    - Views/SettingsPages/TelemetrySettingsPage.axaml
---

The plugin's UI is built entirely with Avalonia UI and the ClassIsland Plugin SDK. There are no custom CSS/SCSS files; styling is done inline in `.axaml` markup through Avalonia's resource system and FluentAvalonia controls.

**Framework & tooling**
- **Avalonia UI** (`https://github.com/avaloniaui`) — declared via the `xmlns="https://github.com/avaloniaui"` namespace on every XAML file.
- **FluentAvalonia** (`FluentAvalonia.UI.Controls`, aliased as `fa:`) — provides settings-oriented controls such as `SettingsExpander`, `ComboBox`, etc., used throughout all Settings pages.
- **ClassIsland Plugin SDK** (`http://classisland.tech/schemas/xaml/core`, aliased as `ci:`) — supplies base types like `ComponentBase<T>` and `SettingsPageBase`, plus helper controls like `IconText` and `FluentIconSource`.
- No external theme package or design-token library is referenced; the project targets `net8.0-windows` and relies on the host (ClassIsland) to supply the runtime theme resources.

**Resource usage**
- The only explicit styling references are Avalonia/Fluent system brushes, consumed via `{DynamicResource ...}`:
  - `SystemFillColorSubtleBackgroundBrush` (used for ID badge backgrounds)
  - `SystemControlForegroundBaseMediumBrush` (used for muted foreground text)
- All other visual properties (spacing, alignment, visibility, opacity, colors) are set directly as attributes on controls rather than through reusable styles.

**Layout conventions**
- Settings pages follow a consistent pattern: a root `Grid` containing a `ScrollViewer` with `Padding="16"`, inside which a `StackPanel Spacing="8" Classes="settings-container animated-intro"` holds sections of `fa:SettingsExpander` blocks.
- Each expander has an `IconSource` bound to a Fluent glyph via `ci:FluentIconSource`, a `Header`, optional `Description`, and a `Footer` that hosts action buttons.
- Data entry uses `TextBox` with `Watermark` placeholders and `MinWidth` constraints; lists use `ItemsControl` with a `DataTemplate` per entry.
- Components (e.g. `AiTextComponent`) are minimal — typically a `Panel` stacking a primary `TextBlock` over a placeholder `TextBlock` whose `Opacity` and `IsVisible` are toggled based on content presence.

**Conventions developers should follow**
- Use `ci:` namespace for ClassIsland base types and `fa:` for FluentAvalonia controls; avoid raw Avalonia primitives unless necessary.
- Prefer `{DynamicResource System...}` brushes over hard-coded colors so the UI adapts to the host theme.
- Keep component XAML flat and declarative; bind data through Avalonia `StyledProperty`s exposed on the code-behind class.
- Do not introduce separate `.css`/`.scss` files — this repo does not include any stylesheet pipeline.