using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Threading.Tasks;
using VideoTagger.Models;
using VideoTagger.Services;

namespace VideoTagger.ViewModels;

public sealed partial class SettingsPageViewModel(
    MainModel mainModel, DbService dbService, DialogService dialogService)
    : ViewModelBase
{
    public MainModel MainModel => mainModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSelectedCateogoryCommand))]
    public partial MainModelCategory? SelectedCategory { get; set; }

    [RelayCommand]
    async Task AddNewCategory()
    {
        if (await dialogService.InputValue<string>("New Category") is { } newCategory)
        {
            var anyChanged = false;
            if (!MainModel.Categories.Any(c => c.Name.Equals(newCategory, System.StringComparison.CurrentCultureIgnoreCase)))
            {
                MainModel.Categories.Add(new() { Name = newCategory });
                anyChanged = true;
            }

            if (anyChanged)
                dbService.WriteMainModel(MainModel);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemoveSelectedCateogory))]
    async Task RemoveSelectedCateogory()
    {
        if (SelectedCategory is null) return;

        if(await dialogService.Question("Remove Category", $"Are you sure you want to remove the category '{SelectedCategory.Name}'?"))
        {
            MainModel.Categories.Remove(SelectedCategory);
            dbService.WriteMainModel(MainModel);
            SelectedCategory = null;
        }
    }

    bool CanRemoveSelectedCateogory() =>
        SelectedCategory is not null;
}
