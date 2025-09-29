using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VideoTagger.Models;
using VideoTagger.Services;

namespace VideoTagger.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(MainModel mainModel, DbService dbService)
    {
        dbService.FillMainModel(mainModel);
    }

    [ObservableProperty]
    public partial bool ShowSettingsPage { get; set; } = true;

    [RelayCommand]
    void SelectSettingsPage()
    {
        ShowSettingsPage = true;
    }
}
