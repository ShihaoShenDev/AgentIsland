using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;

namespace AgentIsland.Shared;

public static class InjectService
{
    public static bool TryGetAddSettingsPageGroupMethod([MaybeNullWhen(false)] out MethodInfo method)
    {
        Type settingsWindowRegistryExtensionsType = typeof(SettingsWindowRegistryExtensions);
        method = settingsWindowRegistryExtensionsType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => m.Name == "AddSettingsPageGroup" && m.GetParameters().Length == 4);
        return method != null;
    }

    public static bool TryGetSettingsPageInfoGroupIdProperty([MaybeNullWhen(false)] out PropertyInfo property)
    {
        Type settingsPageInfoType = typeof(SettingsPageInfo);
        property = settingsPageInfoType
            .GetProperties()
            .FirstOrDefault(p => p.Name == "GroupId" && p.PropertyType == typeof(string));
        return property != null;
    }
}
