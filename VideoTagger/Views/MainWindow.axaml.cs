using Avalonia.Controls;
using FluentAvalonia.UI.Windowing;
using VideoTagger.ViewModels;

namespace VideoTagger.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        TransparencyLevelHint = [WindowTransparencyLevel.Mica];
        InitializeComponent();

        SearchPageView.DataContext = App.GetRequiredService<SearchPageViewModel>();
        SettingsPageView.DataContext = App.GetRequiredService<SettingsPageViewModel>();
    }
}