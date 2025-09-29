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

public sealed class DbService : IHostedService
{
    readonly ILiteDatabase db;
    readonly ILiteCollection<MainModelCategory> categoriesCollection;

    public DbService()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VideoTagger", "videotagger.db");
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dbPath)!);

        db = new LiteDatabase(new ConnectionString(dbPath)
        {
            AutoRebuild = true,
            Connection = ConnectionType.Shared,
            Upgrade = true
        });
        categoriesCollection = db.GetCollection<MainModelCategory>();
    }

    public void FillMainModel(MainModel mainModel)
    {
        mainModel.Categories.Clear();
        mainModel.Categories.AddRange(categoriesCollection.FindAll());
    }

    public void WriteMainModel(MainModel mainModel)
    {
        categoriesCollection.DeleteAll();
        categoriesCollection.InsertBulk(mainModel.Categories);
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