using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace lols.android;

[Activity(Label = "lols", Theme = "@style/MyTheme.NoActionBar", Icon = "@drawable/icon", LaunchMode = LaunchMode.SingleInstance, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
public class MainActivity : AvaloniaMainActivity
{
}
