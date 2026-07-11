using AgentIsland.Models;
using Avalonia.Interactivity;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;

namespace AgentIsland.Views.SettingsPages;

[SettingsPageInfo(
    id: "agentisland.aitext",
    name: "AI 文字",
    category: SettingsPageCategory.External
)]
public partial class AiTextSettingsPage : SettingsPageBase
{
    public AiTextSettingsPage()
    {
        InitializeComponent();
        DataContext = Plugin.Settings;
    }

    private void OnAddEntry(object? sender, RoutedEventArgs e)
    {
        Plugin.Settings.AiTextEntries.Add(new AiTextEntry
        {
            Id = $"text{Plugin.Settings.AiTextEntries.Count + 1}"
        });
    }

    private void OnDeleteEntry(object? sender, RoutedEventArgs e)
    {
        if (sender is Avalonia.Controls.Button { Tag: AiTextEntry entry })
            Plugin.Settings.AiTextEntries.Remove(entry);
    }
}
