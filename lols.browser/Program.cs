using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Web;

[assembly:SupportedOSPlatform("browser")]

namespace lols.browser;

internal class Program
{
    // ReSharper disable once UnusedParameter.Local
    private static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .SetupBrowserApp("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
