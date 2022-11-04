﻿using Avalonia;
using System;

namespace lols;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            //.UsePlatformDetect()
            .UseWin32()
            //.UseSkia()
            .UseDirect2D1()
            .With(new Win32PlatformOptions()
            {
                UseWindowsUIComposition = false
            })
            .LogToTrace();
}
