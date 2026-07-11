using CommunityToolkit.Mvvm.ComponentModel;

namespace AgentIsland.Models;

public partial class AiTextEntry : ObservableObject
{
    [ObservableProperty]
    private string _id = "";

    [ObservableProperty]
    private string _description = "";

    [ObservableProperty]
    private string _text = "";

    public string DisplayName => string.IsNullOrWhiteSpace(Description) ? Id : Description;

    public bool HasNoDescription => string.IsNullOrWhiteSpace(Description);

    partial void OnIdChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayName));
    }

    partial void OnDescriptionChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(HasNoDescription));
    }
}
