using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using VideoTagger.Models;

namespace VideoTagger.ViewModels;

public sealed partial class SearchPageViewModel : ViewModelBase
{
    public partial class MemberSearchItem : ObservableObject
    {
        public required MainModelGroup Group { get; init; }
        public required MainModelGroupMember Member { get; init; }

        [ObservableProperty]
        public partial bool IsSelected { get; set; }
    }

    readonly ReadOnlyObservableCollection<MemberSearchItem> memberSearchItems;
    public ReadOnlyObservableCollection<MemberSearchItem> MemberSearchItems => memberSearchItems;

    public SearchPageViewModel(MainModel mainModel)
    {
        mainModel.Groups.ToObservableChangeSet()
            .AutoRefresh()
            .TransformMany(x => x.Members)
            .AutoRefresh()
            .Transform(x => new MemberSearchItem { Group = x.Group, Member = x }, true)
            .Bind(out memberSearchItems)
            .Subscribe();
    }
}
