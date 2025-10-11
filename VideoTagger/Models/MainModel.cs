using CommunityToolkit.Mvvm.ComponentModel;
using LiteDB;
using System;
using System.Collections.ObjectModel;

namespace VideoTagger.Models;

public sealed partial class MainModel : ObservableObject
{
    public ObservableCollection<MainModelCategory> Categories { get; } = [];
    public ObservableCollection<MainModelFolder> Folders { get; } = [];
    public ObservableCollection<MainModelGroup> Groups { get; } = [];
    public ObservableCollection<MainModelVideoCache> VideoCache { get; } = [];
}

public sealed partial class MainModelVideoCache : ObservableObject
{
    [BsonId]
    public int Id { get; set; }

    [ObservableProperty]
    public partial string Path { get; set; }

    [ObservableProperty]
    public partial DateTime Date { get; set; }

    [ObservableProperty]
    public partial string? CoverImageFileName { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<MainModelVideoCacheTag> Tags { get; set; } = [];
}

public sealed partial class MainModelVideoCacheTag : ObservableObject
{
    [ObservableProperty]
    [BsonRef]
    public partial MainModelGroup? Group { get; set; }

    [ObservableProperty]
    [BsonRef]
    public partial MainModelGroupMember? Member { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<MainModelVideoCacheTagItem> Items { get; set; } = [];
}

public sealed partial class MainModelVideoCacheTagItem : ObservableObject
{
    [ObservableProperty]
    [BsonRef]
    public partial MainModelCategory? Category { get; set; }

    [ObservableProperty]
    [BsonRef]
    public partial MainModelCategoryItem? Item { get; set; }

    [ObservableProperty]
    public partial bool BooleanValue { get; set; }

    [ObservableProperty]
    [BsonRef]
    public partial MainModelCategoryItemEnumValue? EnumValue { get; set; }
}

public sealed partial class MainModelCategory : ObservableObject
{
    [ObservableProperty]
    public partial int Id { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    [BsonRef]
    public partial ObservableCollection<MainModelCategoryItem> Items { get; set; } = [];
}

public sealed partial class MainModelCategoryItem : ObservableObject
{
    [BsonId]
    public int Id { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial bool IsBoolean { get; set; }

    [ObservableProperty]
    public partial string? BooleanRegex { get; set; }

    [ObservableProperty]
    [BsonRef]
    public partial ObservableCollection<MainModelCategoryItemEnumValue> EnumValues { get; set; } = [];
}

public sealed partial class MainModelCategoryItemEnumValue : ObservableObject
{
    [BsonId]
    public int Id { get; set; }

    [ObservableProperty]
    public partial string EnumValue { get; set; }

    [ObservableProperty]
    public partial string? Regex { get; set; }
}

public sealed partial class MainModelFolder : ObservableObject
{
    [BsonId]
    public int Id { get; set; }

    [ObservableProperty]
    public partial string Path { get; set; }
}

public sealed partial class MainModelGroup : ObservableObject
{
    [BsonId]
    public int Id { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<MainModelGroupAlternativeName> AlternativeNames { get; set; } = [];

    [ObservableProperty]
    [BsonRef]
    public partial ObservableCollection<MainModelGroupMember> Members { get; set; } = [];
}

public sealed partial class MainModelGroupMember : ObservableObject
{
    [BsonId]
    public int Id { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<MainModelGroupMemberAlternativeName> AlternativeNames { get; set; } = [];
}

public sealed partial class MainModelGroupAlternativeName : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }
}

public sealed partial class MainModelGroupMemberAlternativeName : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }
}