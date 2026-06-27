using System.Collections.Specialized;
using System.ComponentModel;
using AgentIsland.Models;
using Avalonia;
using Avalonia.Controls;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;

namespace AgentIsland.Components;

[ComponentInfo(
    "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
    "AI 文字",
    "\uE29B",
    "由 AI 管理的文字组件，通过 MCP 工具 set_component_text 更新内容。")]
public partial class AiTextComponent : ComponentBase<AiTextComponentSettings>
{
    public static readonly StyledProperty<string> ResolvedTextProperty =
        AvaloniaProperty.Register<AiTextComponent, string>(nameof(ResolvedText));

    public static readonly StyledProperty<string> PlaceholderTextProperty =
        AvaloniaProperty.Register<AiTextComponent, string>(nameof(PlaceholderText));

    public string ResolvedText
    {
        get => GetValue(ResolvedTextProperty);
        private set => SetValue(ResolvedTextProperty, value);
    }

    public string PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        private set => SetValue(PlaceholderTextProperty, value);
    }

    public AiTextComponent()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            Plugin.Settings.AiTextEntries.CollectionChanged += OnEntriesChanged;
            foreach (var entry in Plugin.Settings.AiTextEntries)
                entry.PropertyChanged += OnEntryPropertyChanged;
            if (Settings is not null)
                Settings.PropertyChanged += OnSettingsPropertyChanged;
            UpdateText();
        };
        Unloaded += (_, _) =>
        {
            Plugin.Settings.AiTextEntries.CollectionChanged -= OnEntriesChanged;
            foreach (var entry in Plugin.Settings.AiTextEntries)
                entry.PropertyChanged -= OnEntryPropertyChanged;
            if (Settings is not null)
                Settings.PropertyChanged -= OnSettingsPropertyChanged;
        };
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e) => UpdateText();

    private void OnEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (AiTextEntry entry in e.OldItems)
                entry.PropertyChanged -= OnEntryPropertyChanged;
        if (e.NewItems != null)
            foreach (AiTextEntry entry in e.NewItems)
                entry.PropertyChanged += OnEntryPropertyChanged;
        UpdateText();
    }

    private void OnEntryPropertyChanged(object? sender, PropertyChangedEventArgs e) => UpdateText();

    private void UpdateText()
    {
        var id = Settings?.EntryId ?? "";
        var entry = Plugin.Settings.AiTextEntries.FirstOrDefault(x => x.Id == id);
        var text = entry?.Text ?? "";
        var hasContent = !string.IsNullOrEmpty(text);
        ResolvedText = hasContent ? text : "";
        PlaceholderText = Settings?.PlaceholderText ?? "暂无内容";
        if (PlaceholderTextBlock is not null)
            PlaceholderTextBlock.IsVisible = !hasContent;
    }
}
