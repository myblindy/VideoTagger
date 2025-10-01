using CommunityToolkit.Mvvm.ComponentModel;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VideoTagger.Helpers;
using VideoTagger.Models;

namespace VideoTagger.Services;

public sealed class DbService : ObservableObject, IHostedService
{
    readonly ILiteDatabase db;
    readonly ILiteCollection<MainModelCategory> categoriesCollection;
    readonly ILiteCollection<MainModelFolder> foldersCollection;
    readonly ILiteCollection<MainModelGroup> groupsCollection;

    class MiscInternal
    {
        public int Id { get; set; }
        public bool IsDirty { get; set; }
    }
    readonly ILiteCollection<MiscInternal> miscInternalCollection;

    public DbService()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VideoTagger", "videotagger.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        db = new LiteDatabase(new ConnectionString(dbPath)
        {
            AutoRebuild = true,
            Connection = ConnectionType.Shared,
            Upgrade = true
        });
        categoriesCollection = db.GetCollection<MainModelCategory>();
        foldersCollection = db.GetCollection<MainModelFolder>();
        groupsCollection = db.GetCollection<MainModelGroup>();
        miscInternalCollection = db.GetCollection<MiscInternal>();

        // misc properties
        if (miscInternalCollection.FindAll().FirstOrDefault() is { } misc)
        {
            IsDirty = misc.IsDirty;
        }
    }

    public void FillMainModel(MainModel mainModel)
    {
        mainModel.Categories.Clear();
        mainModel.Categories.AddRange(categoriesCollection.FindAll());

        mainModel.Folders.Clear();
        mainModel.Folders.AddRange(foldersCollection.FindAll());

        mainModel.Groups.Clear();
        mainModel.Groups.AddRange(groupsCollection.FindAll());
    }

    public void WriteMainModel(MainModel mainModel)
    {
        categoriesCollection.DeleteAll();
        categoriesCollection.InsertBulk(mainModel.Categories);
        foldersCollection.DeleteAll();
        foldersCollection.InsertBulk(mainModel.Folders);
        groupsCollection.DeleteAll();
        groupsCollection.InsertBulk(mainModel.Groups);

        IsDirty = true;
    }

    public bool IsDirty
    {
        get;
        set
        {
            SetProperty(ref field, value);
            if (miscInternalCollection.FindAll().FirstOrDefault() is not { } misc)
                misc = new();
            misc.IsDirty = value;
            miscInternalCollection.Upsert(misc);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        db.Dispose();
        return Task.CompletedTask;
    }
}

static class DbServiceHostExtensions
{
    public static IServiceCollection AddDbService(this IServiceCollection services) => services
        .AddSingleton<DbService>()
        .AddHostedService(p => p.GetRequiredService<DbService>());
}