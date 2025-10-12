using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using VideoTagger.Models;
using VideoTagger.Services;

namespace VideoTagger.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly VideoProcessingService videoProcessingService;

    public DbService DbService { get; }

    [ObservableProperty]
    public partial bool Updating { get; set; }

    public MainWindowViewModel(MainModel mainModel, DbService dbService, VideoProcessingService videoProcessingService)
    {
        this.videoProcessingService = videoProcessingService;
        DbService = dbService;
        dbService.FillMainModel(mainModel);
    }

    [ObservableProperty]
    public partial bool ShowSettingsPage { get; set; } 

    [ObservableProperty]
    public partial bool ShowSearchPage { get; set; } = true;

    [RelayCommand]
    void SelectSearchPage()
    {
        ShowSearchPage = true;
        ShowSettingsPage = false;
    }

    [RelayCommand]
    void SelectSettingsPage()
    {
        ShowSearchPage = false;
        ShowSettingsPage = true;
    }

    [RelayCommand]
    async Task UpdateDatabase()
    {
        try
        {
            Updating = true;

            await videoProcessingService.UpdateVideosAsync();
            DbService.IsDirty = false;
        }
        finally { Updating = false; }
    }
}
