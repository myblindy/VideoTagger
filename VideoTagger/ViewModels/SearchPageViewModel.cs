using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using VideoTagger.Models;

namespace VideoTagger.ViewModels;

public sealed partial class SearchPageViewModel : ViewModelBase
{
    readonly ReadOnlyObservableCollection<SearchCategoryItem> searchCategoryItems;
    public ReadOnlyObservableCollection<SearchCategoryItem> SearchCategoryItems => searchCategoryItems;

    readonly Subject<bool> searchRequestObservable = new();

    public ObservableCollection<MainModelVideoCacheEntry> SearchResults { get; } = [];

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

        searchRequestObservable.Throttle(TimeSpan.FromSeconds(.5)).Subscribe(_ =>
        {
            SearchResults.Clear();

            foreach (var videoCacheItem in mainModel.VideoCache)
            {
                foreach (var searchCategoryItem in SearchCategoryItems)
                    foreach (var searchItem in searchCategoryItem.Items)
                        if (searchItem.IsSelected)
                            if (videoCacheItem.Tags.FirstOrDefault(t =>
                                t?.Member?.Group?.Name == searchCategoryItem.CategoryName
                                && t.Member?.Name == searchItem.ItemName) is { } memberTag)
                            {
                                // found the group and member, check the other tags
                                // TODO

                            }
                            else
                                goto fail;

                // success
                SearchResults.Add(videoCacheItem);
                continue;

                fail:;
            }
        });
    }

    [RelayCommand]
    void Search() =>
        searchRequestObservable.OnNext(true);
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
