using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Dialogs;
using FluentAvalonia.UI.Windowing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Simple.Avalonia.Hosting;
using System;
using System.IO;
using VideoTagger;
using VideoTagger.Helpers;
using VideoTagger.Models;
using VideoTagger.Services;
using VideoTagger.ViewModels;
using VideoTagger.Views;

var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "VideoTagger", "VideoTagger.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddDbContextPool<DbModel>(options => options
        .UseSqlite($"Data Source={dbPath}"))

    .AddDbService()
    .AddDialogService()
    .AddVideoProcessingService()

    .AddSingleton<MainModel>()

    .AddTransient<InputValueDialogView>()
    .AddTransient<InputValueDialogViewModel>()

    .AddSingleton<SettingsPageViewModel>()
    .AddSingleton<SettingsPageView>()

    .AddSingleton<SearchPageViewModel>()
    .AddSingleton<SearchPageView>()

    .AddSingleton<MainWindowViewModel>();

builder.AddAvaloniaDesktopHost<MainWindow>(BuildAvaloniaAppFromServiceProvider);

var app = builder.Build();
app.Run();

internal sealed partial class Program
{
    /// <summary>
    /// Only used by the visual designer in <see cref="BuildAvaloniaApp"/>
    /// </summary>
    private static readonly IServiceProvider EmptyServiceProvider = new ServiceCollection().BuildServiceProvider();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain / Setup is called: things aren't initialized
    // yet and stuff might break.

    // Avalonia configuration, don't remove; also used by visual designer.
    // ReSharper disable once UnusedMember.Global
    public static AppBuilder BuildAvaloniaApp() => BuildAvaloniaAppFromServiceProvider(EmptyServiceProvider);

    private static AppBuilder BuildAvaloniaAppFromServiceProvider(IServiceProvider serviceProvider)
    {
        var appBuilder = AppBuilder.Configure(() => new App(serviceProvider))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .AfterSetup(builder =>
                {
                    // The ApplicationLifetime is null when using the previewer.
                    if (builder.Instance?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        AfterDesktopSetup(desktop, serviceProvider);
                    }
                });
        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            appBuilder.UseManagedSystemDialogs<AppWindow>();

        return appBuilder;
    }

    private static void AfterDesktopSetup(IClassicDesktopStyleApplicationLifetime desktop, IServiceProvider serviceProvider)
    {
        var mainWindow = serviceProvider.GetRequiredService<Window>();
        var mainWindowViewModel = serviceProvider.GetRequiredService<MainWindowViewModel>();
        mainWindow.DataContext = mainWindowViewModel;
        desktop.MainWindow = mainWindow;
    }
}
