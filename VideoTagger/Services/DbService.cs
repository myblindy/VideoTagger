using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoTagger.Models;

namespace VideoTagger.Services;

public sealed class DbService : ObservableObject
{
    readonly IDbContextFactory<DbModel> dbFactory;
    readonly IMapper mapper;
    readonly AsyncManualResetEvent loadedEvent = new();

    public DbService(IDbContextFactory<DbModel> dbFactory, IMapper mapper)
    {
        this.dbFactory = dbFactory;
        this.mapper = mapper;

        async Task Init()
        {
            using (var db = dbFactory.CreateDbContext())
                await db.Database.MigrateAsync();

            IsDirty = DbMisc.IsDirty;
            loadedEvent.Set();
        }
        _ = Init();
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
        await loadedEvent.WaitAsync();

        using var db = dbFactory.CreateDbContext();

        mainModel.Categories.Clear();
        mainModel.Categories.AddRange(mapper.Map<IList<MainModelCategory>>(
            await db.Categories.Include(c => c.Items).ThenInclude(i => i.EnumValues)
                .ToListAsync()));

        mainModel.Folders.Clear();
        mainModel.Folders.AddRange(mapper.Map<IList<MainModelFolder>>(
            await db.Folders.ToListAsync()));

        mainModel.Groups.Clear();
        mainModel.Groups.AddRange(mapper.Map<IList<MainModelGroup>>(
            await db.Groups.Include(g => g.Members)
                .ToListAsync()));

        mainModel.VideoCache.Clear();
        mainModel.VideoCache.AddRange(mapper.Map<IList<MainModelVideoCacheEntry>>(
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
        await loadedEvent.WaitAsync();

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

        db.VideoCacheEntries.RemoveRange(
            await db.VideoCacheEntries.ToListAsync().ConfigureAwait(false));
        db.VideoCacheEntries.AddRange(mapper.Map<IList<MainModelVideoCacheEntry>, IList<DbModelVideoCacheEntry>>(
            mainModel.VideoCache));

        await db.SaveChangesAsync();

        IsDirty = true;
    }

    readonly ConcurrentBag<(MainModelVideoCacheEntry entry, Func<byte[]?>? coverImageBytesGetter, bool forceCoverImageUpdate)> videoCacheEntryUpdates = [];

    public void QueueVideoCacheEntryUpdate(MainModelVideoCacheEntry entry, Func<byte[]?>? coverImageBytesBuilder = null, bool forceCoverImageUpdate = false) =>
        videoCacheEntryUpdates.Add((entry, coverImageBytesBuilder, forceCoverImageUpdate));

    public async Task CommitVideoCacheEntryUpdatesAsync()
    {
        await loadedEvent.WaitAsync();

        using var db = dbFactory.CreateDbContext();

        var allExistingEntries = await db.VideoCacheEntries.ToDictionaryAsync(x => x.Path, x => x);
        List<Task> coverImageWriteTasks = [];
        List<DbModelVideoCacheEntry> newEntries = [];

        foreach (var (entry, coverImageBytesGetter, forceCoverImageUpdate) in videoCacheEntryUpdates)
        {
            void HandleCoverImageFile(DbModelVideoCacheEntry existingEntry)
            {
                if ((forceCoverImageUpdate || existingEntry.CoverImage?.ImageData is null) && coverImageBytesGetter?.Invoke() is { } imageBytes)
                    existingEntry?.CoverImage = new() { ImageData = imageBytes };
                else if ((forceCoverImageUpdate || existingEntry?.CoverImage?.ImageData is not null) && coverImageBytesGetter is null)
                    existingEntry.CoverImage = null;
            }

            if (allExistingEntries.TryGetValue(entry.Path, out var existingEntry))
            {
                coverImageWriteTasks.Add(Task.Run(() => HandleCoverImageFile(existingEntry)));
            }
            else
            {
                // create the new entry and fix the links (should probably be done in the automapper?)
                var newEntry = mapper.Map<DbModelVideoCacheEntry>(entry);
                foreach (var newEntryTag in newEntry.Tags)
                {
                    newEntryTag.MemberId = await db.GroupMembers
                        .Where(m => m.Name == newEntryTag.Member!.Name && m.Group.Name == newEntryTag.Member.Group.Name)
                        .Select(m => m.Id)
                        .FirstAsync()
                        .ConfigureAwait(false);
                    
                    foreach(var newEntryTagItem in newEntryTag.Items)
                    {
                        newEntryTagItem.CategoryItemId = await db.CategoryItems
                            .Where(i => i.Name == newEntryTagItem.CategoryItem!.Name && i.Category.Name == newEntryTagItem.CategoryItem.Category.Name)
                            .Select(i => i.Id)
                            .FirstAsync()
                            .ConfigureAwait(false);

                        if (newEntryTagItem.EnumValue is not null)
                        {
                            newEntryTagItem.EnumValueId = await db.CategoryItemEnumValues
                                .Where(ev => ev.EnumValue == newEntryTagItem.EnumValue.EnumValue 
                                    && ev.CategoryItem.Name == newEntryTagItem.CategoryItem.Name 
                                    && ev.CategoryItem.Category.Name == newEntryTagItem.CategoryItem.Category.Name)
                                .Select(ev => ev.Id)
                                .FirstAsync()
                                .ConfigureAwait(false);
                        }
                    }
                }

                coverImageWriteTasks.Add(Task.Run(() => HandleCoverImageFile(newEntry)));
                newEntries.Add(newEntry);
            }
        }

        await Task.WhenAll(coverImageWriteTasks).ConfigureAwait(false);
        db.VideoCacheEntries.AddRange(newEntries);
        await db.SaveChangesAsync().ConfigureAwait(false);

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