using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace lols;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
