using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoTagger.Helpers;
using VideoTagger.Models;

namespace VideoTagger.Services;

public sealed class DbService : ObservableObject
{
    readonly IDbContextFactory<DbModel> dbFactory;
    readonly IMapper mapper;

    public DbService(IDbContextFactory<DbModel> dbFactory, IMapper mapper)
    {
        this.dbFactory = dbFactory;
        this.mapper = mapper;
        using (var db = dbFactory.CreateDbContext())
            db.Database.Migrate();

        IsDirty = DbMisc.IsDirty;
    }

    DbMisc DbMisc
    {
        get
        {
            using var db = dbFactory.CreateDbContext();
            if (db.Set<DbMisc>().FirstOrDefault() is not { } misc)
            {
                misc = new()
                {
                    DatabaseVersion = DbMisc.CurrentDatabaseVersion,
                    IsDirty = false
                };
                db.Set<DbMisc>().Add(misc);
                db.SaveChanges();
            }
            return misc;
        }
    }

    public async Task FillMainModel(MainModel mainModel)
    {
        using var db = dbFactory.CreateDbContext();

        mainModel.Categories.Clear();
        mainModel.Categories.AddRange(mapper.Map<List<DbModelCategory>, List<MainModelCategory>>(
            await db.Categories.Include(c => c.Items).ThenInclude(i => i.EnumValues)
                .ToListAsync()));

        mainModel.Folders.Clear();
        mainModel.Folders.AddRange(mapper.Map<List<DbModelFolder>, List<MainModelFolder>>(
            await db.Folders.ToListAsync()));

        mainModel.Groups.Clear();
        mainModel.Groups.AddRange(mapper.Map<List<DbModelGroup>, List<MainModelGroup>>(
            await db.Groups.Include(g => g.Members)
                .ToListAsync()));

        mainModel.VideoCache.Clear();
        mainModel.VideoCache.AddRange(mapper.Map<List<DbModelVideoCacheEntry>, List<MainModelVideoCacheEntry>>(
            await db.VideoCacheEntries.Include(v => v.Tags).ThenInclude(t => t.Items)//.ThenInclude(i => i.CategoryItem)
                .Include(v => v.Tags).ThenInclude(t => t.Items).ThenInclude(i => i.EnumValue)
                .Include(v => v.Tags).ThenInclude(t => t.Member)
                .ToListAsync()));

        foreach (var group in mainModel.Groups)
            foreach (var member in group.Members)
                member.Group = group;
    }

    public async Task WriteMainModel(MainModel mainModel)
    {
        using var db = dbFactory.CreateDbContext();

        db.CategoryItemEnumValues.RemoveRange(
            await db.CategoryItemEnumValues.ToListAsync().ConfigureAwait(false));
        db.CategoryItems.RemoveRange(
            await db.CategoryItems.ToListAsync().ConfigureAwait(false));
        db.Categories.RemoveRange(
            await db.Categories.ToListAsync().ConfigureAwait(false));
        db.Categories.AddRange(mapper.Map<IList<MainModelCategory>, IList<DbModelCategory>>(
            mainModel.Categories));

        db.Folders.RemoveRange(
            await db.Folders.ToListAsync().ConfigureAwait(false));
        db.Folders.AddRange(mapper.Map<IList<MainModelFolder>, IList<DbModelFolder>>(
            mainModel.Folders));

        db.GroupMembers.RemoveRange(
            await db.GroupMembers.ToListAsync().ConfigureAwait(false));
        db.Groups.RemoveRange(
            await db.Groups.ToListAsync().ConfigureAwait(false));
        db.Groups.AddRange(mapper.Map<IList<MainModelGroup>, IList<DbModelGroup>>(
            mainModel.Groups));

        //videoCacheCollection.DeleteAll();
        //videoCacheCollection.Insert(mainModel.VideoCache);

        IsDirty = true;
    }

    readonly ConcurrentBag<(MainModelVideoCacheEntry entry, Func<byte[]?>? coverImageBytesGetter, bool forceCoverImageUpdate)> videoCacheEntryUpdates = [];

    public void QueueVideoCacheEntryUpdate(MainModelVideoCacheEntry entry, Func<byte[]?>? coverImageBytesGetter = null, bool forceCoverImageUpdate = false) =>
        videoCacheEntryUpdates.Add((entry, coverImageBytesGetter, forceCoverImageUpdate));

    public async Task CommitVideoCacheEntryUpdatesAsync()
    {
        //var allExistingEntries = videoCacheCollection.FindAll().ToDictionary(x => x.Path, x => x);
        //List<MainModelVideoCacheEntry> updates = [], inserts = [];
        //List<Task> coverImageWriteTasks = [];

        //foreach (var (entry, coverImageBytesGetter, forceCoverImageUpdate) in videoCacheEntryUpdates)
        //{
        //    void HandleCoverImageFile(MainModelVideoCacheEntry? existingEntry)
        //    {
        //        if ((forceCoverImageUpdate || existingEntry?.CoverImageFileName is null) && coverImageBytesGetter?.Invoke() is { } imageBytes)
        //        {
        //            entry.CoverImageFileName ??= existingEntry?.CoverImageFileName
        //                ?? Guid.NewGuid().ToString("N") + ".jpg";
        //            db.FileStorage.Upload(entry.CoverImageFileName, entry.CoverImageFileName, new MemoryStream(imageBytes));
        //        }
        //        else if ((forceCoverImageUpdate || existingEntry?.CoverImageFileName is not null) && coverImageBytesGetter is null)
        //        {
        //            if (existingEntry?.CoverImageFileName is { } existingCoverImageFileName)
        //                db.FileStorage.Delete(existingCoverImageFileName);
        //            entry.CoverImageFileName = null;
        //        }
        //        else
        //            entry.CoverImageFileName = existingEntry?.CoverImageFileName;
        //    }

        //    if (allExistingEntries.TryGetValue(entry.Path, out var existingEntry))
        //    {
        //        entry.Id = existingEntry.Id;
        //        coverImageWriteTasks.Add(Task.Run(() => HandleCoverImageFile(existingEntry)));
        //        updates.Add(entry);
        //    }
        //    else
        //    {
        //        coverImageWriteTasks.Add(Task.Run(() => HandleCoverImageFile(null)));
        //        inserts.Add(entry);
        //    }
        //}

        //await Task.WhenAll(coverImageWriteTasks).ConfigureAwait(false);
        //videoCacheCollection.InsertBulk(inserts);
        //videoCacheCollection.Update(updates);

        videoCacheEntryUpdates.Clear();
        IsDirty = false;
    }

    public bool IsDirty
    {
        get;
        set
        {
            SetProperty(ref field, value);

            using var db = dbFactory.CreateDbContext();
            var misc = DbMisc;
            db.Attach(misc);
            misc.IsDirty = value;
            db.SaveChanges();
        }
    }
}

static class DbServiceHostExtensions
{
    public static IServiceCollection AddDbService(this IServiceCollection services) => services
        .AddSingleton<DbService>();
}