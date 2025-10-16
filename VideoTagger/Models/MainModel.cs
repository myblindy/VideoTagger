using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData.Binding;
using System;

namespace VideoTagger.Models;

public sealed partial class MainModel : ObservableObject
{
    public ObservableCollectionExtended<MainModelCategory> Categories { get; } = [];
    public ObservableCollectionExtended<MainModelFolder> Folders { get; } = [];
    public ObservableCollectionExtended<MainModelGroup> Groups { get; } = [];
    public ObservableCollectionExtended<MainModelVideoCacheEntry> VideoCache { get; } = [];
}

public sealed partial class MainModelVideoCacheEntry : ObservableObject
{
    [ObservableProperty]
    public partial string Path { get; set; }

    [ObservableProperty]
    public partial DateTime Date { get; set; }

    public ObservableCollectionExtended<MainModelVideoCacheTag> Tags { get; } = [];
}

public sealed partial class MainModelVideoCacheTag : ObservableObject
{
    [ObservableProperty]
    public partial MainModelGroupMember? Member { get; set; }

    [ObservableProperty]
    public partial ObservableCollectionExtended<MainModelVideoCacheTagItem> Items { get; set; } = [];
}

public sealed partial class MainModelVideoCacheTagItem : ObservableObject
{
    [ObservableProperty]
    public partial MainModelCategoryItem? CategoryItem { get; set; }

    [ObservableProperty]
    public partial bool BooleanValue { get; set; }

    [ObservableProperty]
    public partial MainModelCategoryItemEnumValue? EnumValue { get; set; }
}

public sealed partial class MainModelCategory : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }

    public ObservableCollectionExtended<MainModelCategoryItem> Items { get; } = [];
}

public sealed partial class MainModelCategoryItem : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial bool IsBoolean { get; set; }

    [ObservableProperty]
    public partial string? BooleanRegex { get; set; }

    public ObservableCollectionExtended<MainModelCategoryItemEnumValue> EnumValues { get; } = [];
}

public sealed partial class MainModelCategoryItemEnumValue : ObservableObject
{
    [ObservableProperty]
    public partial string EnumValue { get; set; } = "";

    [ObservableProperty]
    public partial string? Regex { get; set; }
}

public sealed partial class MainModelFolder : ObservableObject
{
    [ObservableProperty]
    public partial string Path { get; set; }
}

public sealed partial class MainModelGroup : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial ObservableCollectionExtended<string> AlternativeNames { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollectionExtended<MainModelGroupMember> Members { get; set; } = [];
}

public sealed partial class MainModelGroupMember : ObservableObject
{
    [ObservableProperty]
    public partial MainModelGroup Group { get; set; } = null!;

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial ObservableCollectionExtended<string> AlternativeNames { get; set; } = [];
}
