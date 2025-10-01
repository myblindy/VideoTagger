using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace VideoTagger.Models;

public sealed partial class MainModel : ObservableObject
{
    public ObservableCollection<MainModelCategory> Categories { get; } = [];
    public ObservableCollection<MainModelFolder> Folders { get; } = [];
    public ObservableCollection<MainModelGroup> Groups { get; } = [];
}

public sealed partial class MainModelCategory : ObservableObject
{
    [ObservableProperty]
    public partial int Id { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<MainModelCategoryItem> Items { get; set; } = [];
}

public sealed partial class MainModelCategoryItem : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial bool IsBoolean { get; set; }

    [ObservableProperty]
    public partial string? BooleanRegex { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<MainModelCategoryItemEnumValue> EnumValues { get; set; } = [];
}

public sealed partial class MainModelCategoryItemEnumValue : ObservableObject
{
    [ObservableProperty]
    public partial string EnumValue { get; set; }

    [ObservableProperty]
    public partial string? Regex { get; set; }
}

public sealed partial class MainModelFolder : ObservableObject
{
    [ObservableProperty]
    public partial int Id { get; set; }

    [ObservableProperty]
    public partial string Path { get; set; }
}

public sealed partial class MainModelGroup : ObservableObject
{
    [ObservableProperty]
    public partial int Id { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<MainModelGroupMember> Members { get; set; } = [];
}

public sealed partial class MainModelGroupMember : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }
}