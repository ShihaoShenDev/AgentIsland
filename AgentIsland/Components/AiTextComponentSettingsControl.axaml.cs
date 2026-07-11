using System.Collections.Specialized;
using AgentIsland.Models;
using ClassIsland.Core.Abstractions.Controls;

namespace AgentIsland.Components;

public partial class AiTextComponentSettingsControl : ComponentBase<AiTextComponentSettings>
{
    public AiTextComponentSettingsControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        RefreshItems();
        Plugin.Settings.AiTextEntries.CollectionChanged += OnEntriesCollectionChanged;
        EntryComboBox.SelectionChanged += OnSelectionChanged;
    }

    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Plugin.Settings.AiTextEntries.CollectionChanged -= OnEntriesCollectionChanged;
        EntryComboBox.SelectionChanged -= OnSelectionChanged;
    }

    private void RefreshItems()
    {
        EntryComboBox.ItemsSource = Plugin.Settings.AiTextEntries;
        SyncSelection();
    }

    private void SyncSelection()
    {
        var id = Settings?.EntryId ?? "";
        var entry = Plugin.Settings.AiTextEntries.FirstOrDefault(x => x.Id == id);
        EntryComboBox.SelectedItem = entry;
    }

    private void OnEntriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshItems();

    private void OnSelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        if (Settings is null) return;
        if (EntryComboBox.SelectedItem is AiTextEntry entry)
            Settings.EntryId = entry.Id;
        else
            Settings.EntryId = "";
    }
}
