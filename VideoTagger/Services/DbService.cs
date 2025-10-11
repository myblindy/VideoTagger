using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using LiteDB;
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
    readonly ILiteDatabase db;
    readonly ILiteCollection<MainModelCategory> categoriesCollection;
    readonly ILiteCollection<MainModelFolder> foldersCollection;
    readonly ILiteCollection<MainModelGroup> groupsCollection;
    readonly ILiteCollection<MainModelGroupMember> groupMembersCollection;
    readonly ILiteCollection<MainModelVideoCache> videoCacheCollection;
    readonly ILiteCollection<MainModelCategoryItem> categoryItemsCollection;
    readonly ILiteCollection<MainModelCategoryItemEnumValue> categoryItemEnumValuesCollection;

    class MiscInternal
    {
        public int Id { get; set; }
        public bool IsDirty { get; set; }
    }
    readonly ILiteCollection<MiscInternal> miscInternalCollection;

    public DbService(ILiteDatabase db)
    {
        this.db = db;
        categoriesCollection = db.GetCollection<MainModelCategory>();
        foldersCollection = db.GetCollection<MainModelFolder>();
        groupsCollection = db.GetCollection<MainModelGroup>();
        miscInternalCollection = db.GetCollection<MiscInternal>();
        groupMembersCollection = db.GetCollection<MainModelGroupMember>();
        videoCacheCollection = db.GetCollection<MainModelVideoCache>();
        categoryItemsCollection = db.GetCollection<MainModelCategoryItem>();
        categoryItemEnumValuesCollection = db.GetCollection<MainModelCategoryItemEnumValue>();

        LiteDBHelpers.Register<MainModelCategory, MainModelCategoryItem>(w => w.Items);
        LiteDBHelpers.Register<MainModelCategoryItem, MainModelCategoryItemEnumValue>(w => w.EnumValues);
        LiteDBHelpers.Register<MainModelGroup, MainModelGroupMember>(w => w.Members);
        LiteDBHelpers.Register<MainModelVideoCacheTag, MainModelGroup>(w => w.Group);
        LiteDBHelpers.Register<MainModelVideoCacheTag, MainModelGroupMember>(w => w.Member);
        LiteDBHelpers.Register<MainModelVideoCacheTagItem, MainModelCategory>(w => w.Category);
        LiteDBHelpers.Register<MainModelVideoCacheTagItem, MainModelCategoryItem>(w => w.Item);
        LiteDBHelpers.Register<MainModelVideoCacheTagItem, MainModelCategoryItemEnumValue>(w => w.EnumValue);

        // misc properties
        if (miscInternalCollection.FindAll().FirstOrDefault() is { } misc)
        {
            IsDirty = misc.IsDirty;
        }
    }

    public void FillMainModel(MainModel mainModel)
    {
        mainModel.Categories.Clear();
        mainModel.Categories.AddRange(categoriesCollection
            .IncludeAll()
            .FindAll());

        mainModel.Folders.Clear();
        mainModel.Folders.AddRange(foldersCollection
            .FindAll());

        mainModel.Groups.Clear();
        mainModel.Groups.AddRange(groupsCollection
            .IncludeAll()
            .FindAll());

        mainModel.VideoCache.Clear();
        mainModel.VideoCache.AddRange(videoCacheCollection
            .IncludeAll()
            .FindAll());
    }

    public void WriteMainModel(MainModel mainModel)
    {
        categoryItemEnumValuesCollection.DeleteAll();
        foreach (var enumValue in mainModel.Categories.SelectMany(x => x.Items.SelectMany(i => i.EnumValues))
            .Union(mainModel.VideoCache.SelectMany(v => v.Tags.SelectMany(t => t.Items.Select(i => i.EnumValue))))
            .Where(w => w is not null))
        {
            enumValue!.Id = 0;
            categoryItemEnumValuesCollection.Insert(enumValue);
        }

        categoryItemsCollection.DeleteAll();
        foreach (var item in mainModel.Categories.SelectMany(x => x.Items)
            .Union(mainModel.VideoCache.SelectMany(x => x.Tags.SelectMany(t => t.Items.Select(i => i.Item)))))
        {
            item!.Id = 0;
            categoryItemsCollection.Insert(item);
        }

        categoriesCollection.DeleteAll();
        categoriesCollection.InsertBulk(mainModel.Categories);

        foldersCollection.DeleteAll();
        foldersCollection.InsertBulk(mainModel.Folders);

        groupMembersCollection.DeleteAll();
        foreach (var member in mainModel.Groups.SelectMany(x => x.Members)
            .Union(mainModel.VideoCache.SelectMany(x => x.Tags.SelectMany(t => t.Group!.Members))))
        {
            member.Id = 0;
            groupMembersCollection.Insert(member);
        }

        groupsCollection.DeleteAll();
        groupsCollection.Insert(mainModel.Groups);

        videoCacheCollection.DeleteAll();
        videoCacheCollection.Insert(mainModel.VideoCache);

        IsDirty = true;
    }

    readonly ConcurrentBag<(MainModelVideoCache entry, Func<byte[]?>? coverImageBytesGetter, bool forceCoverImageUpdate)> videoCacheEntryUpdates = [];
    public void QueueVideoCacheEntryUpdate(MainModelVideoCache entry, Func<byte[]?>? coverImageBytesGetter = null, bool forceCoverImageUpdate = false) =>
        videoCacheEntryUpdates.Add((entry, coverImageBytesGetter, forceCoverImageUpdate));

    public async Task CommitVideoCacheEntryUpdatesAsync()
    {
        var allExistingEntries = videoCacheCollection.FindAll().ToDictionary(x => x.Path, x => x);
        List<MainModelVideoCache> updates = [], inserts = [];
        List<Task> coverImageWriteTasks = [];

        foreach (var (entry, coverImageBytesGetter, forceCoverImageUpdate) in videoCacheEntryUpdates)
        {
            void HandleCoverImageFile(MainModelVideoCache? existingEntry)
            {
                if ((forceCoverImageUpdate || existingEntry?.CoverImageFileName is null) && coverImageBytesGetter?.Invoke() is { } imageBytes)
                {
                    entry.CoverImageFileName ??= existingEntry?.CoverImageFileName
                        ?? Guid.NewGuid().ToString("N") + ".jpg";
                    db.FileStorage.Upload(entry.CoverImageFileName, entry.CoverImageFileName, new MemoryStream(imageBytes));
                }
                else if ((forceCoverImageUpdate || existingEntry?.CoverImageFileName is not null) && coverImageBytesGetter is null)
                {
                    if (existingEntry?.CoverImageFileName is { } existingCoverImageFileName)
                        db.FileStorage.Delete(existingCoverImageFileName);
                    entry.CoverImageFileName = null;
                }
                else
                    entry.CoverImageFileName = existingEntry?.CoverImageFileName;
            }

            if (allExistingEntries.TryGetValue(entry.Path, out var existingEntry))
            {
                entry.Id = existingEntry.Id;
                coverImageWriteTasks.Add(Task.Run(() => HandleCoverImageFile(existingEntry)));
                updates.Add(entry);
            }
            else
            {
                coverImageWriteTasks.Add(Task.Run(() => HandleCoverImageFile(null)));
                inserts.Add(entry);
            }
        }

        await Task.WhenAll(coverImageWriteTasks).ConfigureAwait(false);
        videoCacheCollection.InsertBulk(inserts);
        videoCacheCollection.Update(updates);

        videoCacheEntryUpdates.Clear();
        IsDirty = false;
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
}

static class DbServiceHostExtensions
{
    public static IServiceCollection AddDbService(this IServiceCollection services) => services
        .AddSingleton<DbService>();
}