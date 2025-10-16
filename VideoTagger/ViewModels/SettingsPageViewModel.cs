using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using System;
using System.Formats.Asn1;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using VideoTagger.Models;
using VideoTagger.Services;

namespace VideoTagger.ViewModels;

public sealed partial class SettingsPageViewModel : ViewModelBase
{
    readonly MainModel mainModel;
    readonly DbService dbService;
    readonly DialogService dialogService;

    public SettingsPageViewModel(MainModel mainModel, DbService dbService, DialogService dialogService)
    {
        this.mainModel = mainModel;
        this.dbService = dbService;
        this.dialogService = dialogService;

        this.WhenAnyValue(x => x.SelectedCategoryItem!.IsBoolean).Subscribe(_ =>
        {
            AddNewCategoryItemEnumValueCommand.NotifyCanExecuteChanged();
            RemoveSelectedCateogoryEnumValueCommand.NotifyCanExecuteChanged();
            MoveSelectedCategoryDownEnumValueCommand.NotifyCanExecuteChanged();
            MoveSelectedCategoryUpEnumValueCommand.NotifyCanExecuteChanged();
        });

        this.WhenAnyValue(x => x.SelectedCategoryItem!.BooleanRegex, x => x.SelectedCategoryItemEnumValue!.Regex).Throttle(TimeSpan.FromSeconds(1)).Subscribe(async _ =>
        {
            await dbService.WriteMainModel(MainModel);
        });
    }

    public MainModel MainModel => mainModel;

    #region Categories
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSelectedCateogoryCommand), nameof(MoveSelectedCategoryDownCommand), nameof(MoveSelectedCategoryUpCommand))]
    public partial MainModelCategory? SelectedCategory { get; set; }

    partial void OnSelectedCategoryChanged(MainModelCategory? value) =>
        SelectedCategoryItem = value?.Items.FirstOrDefault();

    [RelayCommand]
    async Task AddNewCategory()
    {
        if (await dialogService.InputValue<string>("New Category") is { } newCategory)
        {
            var anyChanged = false;
            if (!MainModel.Categories.Any(c => c.Name.Equals(newCategory, StringComparison.CurrentCultureIgnoreCase)))
            {
                MainModel.Categories.Add(new() { Name = newCategory });
                SelectedCategory = MainModel.Categories[^1];
                anyChanged = true;
            }

            if (anyChanged)
                await dbService.WriteMainModel(MainModel);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemoveSelectedCateogory))]
    async Task RemoveSelectedCateogory()
    {
        if (SelectedCategory is null) return;

        if (await dialogService.Question("Remove Category", $"Are you sure you want to remove the category '{SelectedCategory.Name}'?"))
        {
            MainModel.Categories.Remove(SelectedCategory);
            await dbService.WriteMainModel(MainModel);
            SelectedCategory = null;
        }
    }

    bool CanRemoveSelectedCateogory() =>
        SelectedCategory is not null;

    [RelayCommand(CanExecute = nameof(CanMoveSelectedCategoryUp))]
    async Task MoveSelectedCategoryUp()
    {
        if (SelectedCategory is null) return;

        var index = MainModel.Categories.IndexOf(SelectedCategory);
        if (index > 0)
        {
            var category = SelectedCategory;
            MainModel.Categories.RemoveAt(index);
            MainModel.Categories.Insert(index - 1, category);
            await dbService.WriteMainModel(MainModel);
            SelectedCategory = category;
        }
    }

    bool CanMoveSelectedCategoryUp() =>
        SelectedCategory is not null && MainModel.Categories.IndexOf(SelectedCategory) > 0;

    [RelayCommand(CanExecute = nameof(CanMoveSelectedCategoryDown))]
    async Task MoveSelectedCategoryDown()
    {
        if (SelectedCategory is null) return;

        var index = MainModel.Categories.IndexOf(SelectedCategory);
        if (index < MainModel.Categories.Count - 1)
        {
            var category = SelectedCategory;
            MainModel.Categories.RemoveAt(index);
            MainModel.Categories.Insert(index + 1, category);
            await dbService.WriteMainModel(MainModel);
            SelectedCategory = category;
        }
    }

    bool CanMoveSelectedCategoryDown() =>
        SelectedCategory is not null && MainModel.Categories.IndexOf(SelectedCategory) < MainModel.Categories.Count - 1;
    #endregion

    #region Category Items
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSelectedCategoryItemCommand), nameof(MoveSelectedCategoryItemDownCommand), nameof(MoveSelectedCategoryItemUpCommand))]
    public partial MainModelCategoryItem? SelectedCategoryItem { get; set; }

    partial void OnSelectedCategoryItemChanged(MainModelCategoryItem? value) =>
        SelectedCategoryItemEnumValue = value?.EnumValues.FirstOrDefault();

    [RelayCommand]
    async Task AddNewCategoryItem()
    {
        if (SelectedCategory is null) return;

        if (await dialogService.InputValue<string>("New Category Item") is { } newCategoryItem)
        {
            var anyChanged = false;
            if (!SelectedCategory.Items.Any(i => i.Name.Equals(newCategoryItem, StringComparison.CurrentCultureIgnoreCase)))
            {
                SelectedCategory.Items.Add(new() { Name = newCategoryItem, IsBoolean = true });
                SelectedCategoryItem = SelectedCategory.Items[^1];
                anyChanged = true;
            }
            if (anyChanged)
                await dbService.WriteMainModel(MainModel);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemoveSelectedCategoryItem))]
    async Task RemoveSelectedCategoryItem()
    {
        if (SelectedCategory is null || SelectedCategoryItem is null) return;

        if (await dialogService.Question("Remove Category Item", $"Are you sure you want to remove the category item '{SelectedCategoryItem.Name}' for '{SelectedCategory.Name}'?"))
        {
            SelectedCategory.Items.Remove(SelectedCategoryItem);
            await dbService.WriteMainModel(MainModel);
            SelectedCategoryItem = null;
        }
    }
    bool CanRemoveSelectedCategoryItem() =>
        SelectedCategoryItem is not null;

    [RelayCommand(CanExecute = nameof(CanMoveSelectedCategoryItemUp))]
    async Task MoveSelectedCategoryItemUp()
    {
        if (SelectedCategory is null || SelectedCategoryItem is null) return;
        var index = SelectedCategory.Items.IndexOf(SelectedCategoryItem);
        if (index > 0)
        {
            var categoryItem = SelectedCategoryItem;
            SelectedCategory.Items.RemoveAt(index);
            SelectedCategory.Items.Insert(index - 1, categoryItem);
            await dbService.WriteMainModel(MainModel);
            SelectedCategoryItem = categoryItem;
        }
    }
    bool CanMoveSelectedCategoryItemUp() =>
        SelectedCategoryItem is not null && SelectedCategory is not null && SelectedCategory.Items.IndexOf(SelectedCategoryItem) > 0;

    [RelayCommand(CanExecute = nameof(CanMoveSelectedCategoryItemDown))]
    async Task MoveSelectedCategoryItemDown()
    {
        if (SelectedCategory is null || SelectedCategoryItem is null) return;
        var index = SelectedCategory.Items.IndexOf(SelectedCategoryItem);
        if (index < SelectedCategory.Items.Count - 1)
        {
            var categoryItem = SelectedCategoryItem;
            SelectedCategory.Items.RemoveAt(index);
            SelectedCategory.Items.Insert(index + 1, categoryItem);
            await dbService.WriteMainModel(MainModel);
            SelectedCategoryItem = categoryItem;
        }
    }
    bool CanMoveSelectedCategoryItemDown() =>
        SelectedCategoryItem is not null && SelectedCategory is not null && SelectedCategory.Items.IndexOf(SelectedCategoryItem) < SelectedCategory.Items.Count - 1;
    #endregion

    #region Category Item Enum Values
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSelectedCateogoryEnumValueCommand), nameof(MoveSelectedCategoryDownEnumValueCommand), nameof(MoveSelectedCategoryUpEnumValueCommand))]
    public partial MainModelCategoryItemEnumValue? SelectedCategoryItemEnumValue { get; set; }

    [RelayCommand(CanExecute = nameof(CanAddNewCategoryItemEnumValue))]
    async Task AddNewCategoryItemEnumValue()
    {
        if (SelectedCategoryItem is null) return;

        if (await dialogService.InputValue<string>("New Enum Value") is { } newEnumValue)
        {
            var anyChanged = false;
            if (!SelectedCategoryItem.EnumValues.Any(ev => ev.EnumValue.Equals(newEnumValue, StringComparison.CurrentCultureIgnoreCase)))
            {
                SelectedCategoryItem.EnumValues.Add(new() { EnumValue = newEnumValue });
                SelectedCategoryItemEnumValue = SelectedCategoryItem.EnumValues[^1];
                anyChanged = true;
            }
            if (anyChanged)
                await dbService.WriteMainModel(MainModel);
        }
    }
    bool CanAddNewCategoryItemEnumValue() =>
        SelectedCategoryItem is not null && !SelectedCategoryItem.IsBoolean;

    [RelayCommand(CanExecute = nameof(CanRemoveSelectedCateogoryEnumValue))]
    async Task RemoveSelectedCateogoryEnumValue()
    {
        if (SelectedCategory is null || SelectedCategoryItem is null || SelectedCategoryItemEnumValue is null) return;

        if (await dialogService.Question("Remove Enum Value", $"Are you sure you want to remove the enum value '{SelectedCategoryItemEnumValue}' for '{SelectedCategoryItem.Name}' under the category '{SelectedCategory.Name}'?"))
        {
            SelectedCategoryItem.EnumValues.Remove(SelectedCategoryItemEnumValue);
            await dbService.WriteMainModel(MainModel);
            SelectedCategoryItemEnumValue = null;
        }
    }
    bool CanRemoveSelectedCateogoryEnumValue() =>
        SelectedCategoryItem is not null && SelectedCategoryItemEnumValue is not null && !SelectedCategoryItem.IsBoolean;

    [RelayCommand(CanExecute = nameof(CanMoveSelectedCategoryUpEnumValue))]
    async Task MoveSelectedCategoryUpEnumValue()
    {
        if (SelectedCategoryItem is null || SelectedCategoryItemEnumValue is null) return;
        var index = SelectedCategoryItem.EnumValues.IndexOf(SelectedCategoryItemEnumValue);
        if (index > 0)
        {
            var enumValue = SelectedCategoryItemEnumValue;
            SelectedCategoryItem.EnumValues.RemoveAt(index);
            SelectedCategoryItem.EnumValues.Insert(index - 1, enumValue);
            await dbService.WriteMainModel(MainModel);
            SelectedCategoryItemEnumValue = enumValue;
        }
    }
    bool CanMoveSelectedCategoryUpEnumValue() =>
        SelectedCategoryItem is not null && SelectedCategoryItemEnumValue is not null && !SelectedCategoryItem.IsBoolean && SelectedCategoryItem.EnumValues.IndexOf(SelectedCategoryItemEnumValue) > 0;

    [RelayCommand(CanExecute = nameof(CanMoveSelectedCategoryDownEnumValue))]
    async Task MoveSelectedCategoryDownEnumValue()
    {
        if (SelectedCategoryItem is null || SelectedCategoryItemEnumValue is null) return;
        var index = SelectedCategoryItem.EnumValues.IndexOf(SelectedCategoryItemEnumValue);
        if (index < SelectedCategoryItem.EnumValues.Count - 1)
        {
            var enumValue = SelectedCategoryItemEnumValue;
            SelectedCategoryItem.EnumValues.RemoveAt(index);
            SelectedCategoryItem.EnumValues.Insert(index + 1, enumValue);
            await dbService.WriteMainModel(MainModel);
            SelectedCategoryItemEnumValue = enumValue;
        }
    }
    bool CanMoveSelectedCategoryDownEnumValue() =>
        SelectedCategoryItem is not null && SelectedCategoryItemEnumValue is not null && !SelectedCategoryItem.IsBoolean && SelectedCategoryItem.EnumValues.IndexOf(SelectedCategoryItemEnumValue) < SelectedCategoryItem.EnumValues.Count - 1;
    #endregion

    #region Folders
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSelectedFolderCommand))]
    public partial MainModelFolder? SelectedFolder { get; set; }

    [RelayCommand]
    async Task AddNewFolder()
    {
        if (await dialogService.SelectFolder() is { } folder)
        {
            var anyChanged = false;
            if (!MainModel.Folders.Any(f => f.Path == folder.Path.LocalPath))
            {
                MainModel.Folders.Add(new() { Path = folder.Path.LocalPath });
                SelectedFolder = MainModel.Folders[^1];
                anyChanged = true;
            }
            if (anyChanged)
                await dbService.WriteMainModel(MainModel);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemoveSelectedFolder))]
    async Task RemoveSelectedFolder()
    {
        if (SelectedFolder is null) return;
        if (await dialogService.Question("Remove Folder", $"Are you sure you want to remove the folder '{SelectedFolder.Path}'?"))
        {
            MainModel.Folders.Remove(SelectedFolder);
            await dbService.WriteMainModel(MainModel);
            SelectedFolder = null;
        }
    }
    bool CanRemoveSelectedFolder() =>
        SelectedFolder is not null;
    #endregion

    #region Groups
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSelectedGroupCommand))]
    public partial MainModelGroup? SelectedGroup { get; set; }

    [RelayCommand]
    async Task AddNewGroup()
    {
        if (await dialogService.InputValue<string>("New Group") is { } newGroup)
        {
            var anyChanged = false;
            if (!MainModel.Groups.Any(g => g.Name.Equals(newGroup, StringComparison.CurrentCultureIgnoreCase)))
            {
                MainModel.Groups.Add(new() { Name = newGroup });
                SelectedGroup = MainModel.Groups[^1];
                anyChanged = true;
            }
            if (anyChanged)
                await dbService.WriteMainModel(MainModel);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemoveSelectedGroup))]
    async Task RemoveSelectedGroup()
    {
        if (SelectedGroup is null) return;
        if (await dialogService.Question("Remove Group", $"Are you sure you want to remove the group '{SelectedGroup.Name}'?"))
        {
            MainModel.Groups.Remove(SelectedGroup);
            await dbService.WriteMainModel(MainModel);
            SelectedGroup = null;
        }
    }
    bool CanRemoveSelectedGroup() =>
        SelectedGroup is not null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveGroupAlternativeNameCommand))]
    public partial string? SelectedGroupAlternativeName { get; set; }

    [RelayCommand]
    async Task AddNewGroupAlternativeName()
    {
        if (SelectedGroup is null) return;

        if (await dialogService.InputValue<string>("New Group Alternative Name") is { } newGroupAlternativeName)
        {
            var anyChanged = false;
            if (!SelectedGroup.AlternativeNames.Any(an => an.Equals(newGroupAlternativeName, StringComparison.CurrentCultureIgnoreCase)))
            {
                SelectedGroup.AlternativeNames.Add(newGroupAlternativeName);
                SelectedGroupAlternativeName = SelectedGroup.AlternativeNames[^1];
                anyChanged = true;
            }
            if (anyChanged)
                await dbService.WriteMainModel(MainModel);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemoveGroupAlternativeName))]
    async Task RemoveGroupAlternativeName()
    {
        if (SelectedGroup is null || SelectedGroupAlternativeName is null) return;

        if (await dialogService.Question("Remove Group Alternative Name", $"Are you sure you want to remove the group alternative name '{SelectedGroupAlternativeName}' for '{SelectedGroup.Name}'?"))
        {
            SelectedGroup.AlternativeNames.Remove(SelectedGroupAlternativeName);
            await dbService.WriteMainModel(MainModel);
            SelectedGroupAlternativeName = null;
        }
    }
    bool CanRemoveGroupAlternativeName() =>
        SelectedGroupAlternativeName is not null;
    #endregion

    #region Group Members
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSelectedGroupMemberCommand))]
    public partial MainModelGroupMember? SelectedGroupMember { get; set; }

    [RelayCommand]
    async Task AddNewGroupMember()
    {
        if (SelectedGroup is null) return;

        if (await dialogService.InputValue<string>("New Group Member") is { } newGroupMember)
        {
            var anyChanged = false;
            if (!SelectedGroup.Members.Any(m => m.Name.Equals(newGroupMember, StringComparison.CurrentCultureIgnoreCase)))
            {
                SelectedGroup.Members.Add(new() { Name = newGroupMember, Group = SelectedGroup });
                SelectedGroupMember = SelectedGroup.Members[^1];
                anyChanged = true;
            }
            if (anyChanged)
                await dbService.WriteMainModel(MainModel);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemoveSelectedGroupMember))]
    async Task RemoveSelectedGroupMember()
    {
        if (SelectedGroup is null || SelectedGroupMember is null) return;
        if (await dialogService.Question("Remove Group Member", $"Are you sure you want to remove the group member '{SelectedGroupMember.Name}' from the group '{SelectedGroup.Name}'?"))
        {
            SelectedGroup.Members.Remove(SelectedGroupMember);
            await dbService.WriteMainModel(MainModel);
            SelectedGroupMember = null;
        }
    }
    bool CanRemoveSelectedGroupMember() =>
        SelectedGroupMember is not null;
    #endregion
}
