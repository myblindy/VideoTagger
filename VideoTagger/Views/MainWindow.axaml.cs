using Avalonia.Controls;
using FluentAvalonia.UI.Windowing;
using VideoTagger.ViewModels;

namespace VideoTagger.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();

        SettingsPageView.DataContext = App.GetRequiredService<SettingsPageViewModel>();
        TransparencyLevelHint = [WindowTransparencyLevel.Mica];
    }
}