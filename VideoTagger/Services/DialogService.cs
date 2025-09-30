using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VideoTagger.ViewModels;

namespace VideoTagger.Services;

public sealed partial class DialogService
{
    static Visual RootVisual
    {
        get
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime al && al.MainWindow is not null)
                return al.MainWindow;
            else
                throw new NotImplementedException();
        }
    }

    public async Task<T?> InputValue<T>(string title, T? defaultValue = default)
    {
        Debug.Assert(typeof(T) == typeof(string));  // TODO: Support other types

        var vm = App.GetRequiredService<InputValueDialogViewModel>();
        vm.Value = defaultValue;

        var dialog = new ContentDialog
        {
            Title = title,
            DataContext = vm,
            Content = App.GetRequiredService<InputValueDialogView>(),

            PrimaryButtonText = "OK",
            DefaultButton = ContentDialogButton.Primary,
            CloseButtonText = "Cancel",
        };

        using (vm.WhenAnyValue(x => x.Value).Subscribe(_ =>
            dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(vm.Value as string)))
        {
            if (await dialog.ShowAsync() is ContentDialogResult.Primary)
                return (T?)vm.Value;
        }

        return default;
    }

    public async Task<bool> Question(string title, string message)
    {
        var dialog = new TaskDialog
        {
            Title = "Question",
            Header = title,
            Content = message,
            Buttons = [TaskDialogButton.YesButton, TaskDialogButton.NoButton],
            ShowProgressBar = false,
            IconSource = new SymbolIconSource { Symbol = Symbol.Help },
            FooterVisibility = TaskDialogFooterVisibility.Never,
            IsFooterExpanded = false,
            XamlRoot = RootVisual,
        };
        return await dialog.ShowAsync() is TaskDialogStandardResult.Yes;
    }

    public async Task<IStorageFolder?> SelectFolder(string? title = null) =>
        (await TopLevel.GetTopLevel(RootVisual)!.StorageProvider.OpenFolderPickerAsync(new() { Title = title ?? "Choose a folder" })).FirstOrDefault();
}

static class DialogServiceExtensions
{
    public static IServiceCollection AddDialogService(this IServiceCollection services) => services
        .AddSingleton<DialogService>();
}