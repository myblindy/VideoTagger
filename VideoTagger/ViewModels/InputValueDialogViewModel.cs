using CommunityToolkit.Mvvm.ComponentModel;

namespace VideoTagger.ViewModels;

public partial class InputValueDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial object? Value { get; set; }
}
