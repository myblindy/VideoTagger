using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace VideoTagger.Models;

public sealed partial class MainModel : ObservableObject
{
    public ObservableCollection<MainModelCategory> Categories { get; } = [];
}

public sealed partial class MainModelCategory : ObservableObject
{
    [ObservableProperty]
    public partial int Id { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }
}
