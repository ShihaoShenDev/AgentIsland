using Avalonia.Threading;

namespace AgentIsland.Helpers;

internal static class UiThreadHelper
{
    public static T RunOnUi<T>(Func<T> func)
    {
        return Dispatcher.UIThread.CheckAccess()
            ? func()
            : Dispatcher.UIThread.Invoke(func);
    }

    public static void RunOnUi(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        Dispatcher.UIThread.Invoke(action);
    }
}
