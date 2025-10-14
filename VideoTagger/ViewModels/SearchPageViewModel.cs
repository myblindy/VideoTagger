using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VideoTagger.Models;

namespace VideoTagger.ViewModels;

public sealed partial class SearchPageViewModel : ViewModelBase
{
    readonly ReadOnlyObservableCollection<SearchCategoryItem> searchCategoryItems;
    public ReadOnlyObservableCollection<SearchCategoryItem> SearchCategoryItems => searchCategoryItems;

    public SearchPageViewModel(MainModel mainModel)
    {
        mainModel.Groups.ToObservableChangeSet()
            .AutoRefresh()
            .Transform(x =>
            {
                x.Members.ToObservableChangeSet()
                    .AutoRefresh()
                    .Transform(m => new SearchItem(() => m.Name))
                    .Bind(out var memberSearchItems)
                    .Subscribe();
                return new SearchCategoryItem(() => x.Name, memberSearchItems);
            })
            .MergeChangeSets([
                mainModel.Categories.ToObservableChangeSet()
                    .AutoRefresh()
                    .Transform(x =>
                    {
                        x.Items.ToObservableChangeSet()
                            .AutoRefresh()
                            .Transform(i => new SearchItem(() => i.Name))
                            .Bind(out var categoryItemSearchItems)
                            .Subscribe();
                        return new SearchCategoryItem(() => x.Name, categoryItemSearchItems);
                    })
                ])
            .Bind(out searchCategoryItems)
            .Subscribe();
    }
}

public partial class SearchItem(Func<string> itemNameGetter) : ObservableObject
{
    public string ItemName { get; } = itemNameGetter();

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}

public partial class SearchCategoryItem(Func<string> categoryNameGetter, IList<SearchItem> items)
{
    public string CategoryName { get; } = categoryNameGetter();
    public IList<SearchItem> Items { get; } = items;
}
