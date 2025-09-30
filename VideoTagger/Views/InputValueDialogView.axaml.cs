using Avalonia.Controls;

namespace VideoTagger;

public partial class InputValueDialogView : UserControl
{
    public InputValueDialogView()
    {
        InitializeComponent();
    }

    void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        TextBox.Focus();
}