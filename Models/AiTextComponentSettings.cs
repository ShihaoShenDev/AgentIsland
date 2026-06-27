using CommunityToolkit.Mvvm.ComponentModel;

namespace AgentIsland.Models;

public partial class AiTextComponentSettings : ObservableRecipient
{
    [ObservableProperty]
    private string _entryId = "";

    [ObservableProperty]
    private string _placeholderText = "暂无内容";
}
