using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using System.Collections;
using System.Windows.Input;

namespace VideoTagger.Controls;

public partial class EditableListBox : UserControl
{
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<EditableListBox, string?>(nameof(Header), "Items");
    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<EditableListBox, IEnumerable?>(nameof(ItemsSource));
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<EditableListBox, object?>(nameof(SelectedItem), defaultBindingMode: BindingMode.TwoWay);
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<EditableListBox, IDataTemplate?>(nameof(ItemTemplate));
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly StyledProperty<ICommand?> AddCommandProperty =
        AvaloniaProperty.Register<EditableListBox, ICommand?>(nameof(AddCommand));
    public ICommand? AddCommand
    {
        get => GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> RemoveCommandProperty =
        AvaloniaProperty.Register<EditableListBox, ICommand?>(nameof(RemoveCommand));
    public ICommand? RemoveCommand
    {
        get => GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    public static readonly StyledProperty<bool> IsMoveUpVisibleProperty =
        AvaloniaProperty.Register<EditableListBox, bool>(nameof(IsMoveUpVisible), true);
    public bool IsMoveUpVisible
    {
        get => GetValue(IsMoveUpVisibleProperty);
        set => SetValue(IsMoveUpVisibleProperty, value);
    }

    public static readonly StyledProperty<ICommand?> MoveUpCommandProperty =
        AvaloniaProperty.Register<EditableListBox, ICommand?>(nameof(MoveUpCommand));
    public ICommand? MoveUpCommand
    {
        get => GetValue(MoveUpCommandProperty);
        set => SetValue(MoveUpCommandProperty, value);
    }

    public static readonly StyledProperty<bool> IsMoveDownVisibleProperty =
        AvaloniaProperty.Register<EditableListBox, bool>(nameof(IsMoveDownVisible), true);
    public bool IsMoveDownVisible
    {
        get => GetValue(IsMoveDownVisibleProperty);
        set => SetValue(IsMoveDownVisibleProperty, value);
    }

    public static readonly StyledProperty<ICommand?> MoveDownCommandProperty =
        AvaloniaProperty.Register<EditableListBox, ICommand?>(nameof(MoveDownCommand));
    public ICommand? MoveDownCommand
    {
        get => GetValue(MoveDownCommandProperty);
        set => SetValue(MoveDownCommandProperty, value);
    }

    public static readonly StyledProperty<bool> HasNoItemsProperty =
        AvaloniaProperty.Register<EditableListBox, bool>(nameof(HasNoItems));
    public bool HasNoItems
    {
        get => GetValue(HasNoItemsProperty);
        set => SetValue(HasNoItemsProperty, value);
    }

    public static readonly StyledProperty<string?> NoItemsTextProperty =
        AvaloniaProperty.Register<EditableListBox, string?>(nameof(NoItemsText), "(no items)");
    public string? NoItemsText
    {
        get => GetValue(NoItemsTextProperty);
        set => SetValue(NoItemsTextProperty, value);
    }

    public EditableListBox()
    {
        InitializeComponent();
    }
}