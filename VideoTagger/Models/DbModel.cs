using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace VideoTagger.Models;

public class DbModel(DbContextOptions<DbModel> options) : DbContext(options)
{
    public DbSet<DbMisc> Misc { get; set; } = null!;
    public DbSet<DbModelVideoCacheEntry> VideoCacheEntries { get; set; } = null!;
    public DbSet<DbModelVideoCaccheEntryTag> VideoCacheEntryTags { get; set; } = null!;
    public DbSet<DbModelVideoCacheEntryTagItem> VideoCacheEntryTagItems { get; set; } = null!;
    public DbSet<DbModelCategory> Categories { get; set; } = null!;
    public DbSet<DbModelCategoryItem> CategoryItems { get; set; } = null!;
    public DbSet<DbModelCategoryItemEnumValue> CategoryItemEnumValues { get; set; } = null!;
    public DbSet<DbModelGroup> Groups { get; set; } = null!;
    public DbSet<DbModelGroupMember> GroupMembers { get; set; } = null!;
    public DbSet<DbModelFolder> Folders { get; set; } = null!;
    public DbSet<DbModelVideoCacheCoverImage> VideoCacheCoverImages { get; set; } = null!;
}

public class DbMisc
{
    public const int CurrentDatabaseVersion = 1;
    public int Id { get; set; }
    public int DatabaseVersion { get; set; }
    public bool IsDirty { get; set; }
}

[Index(nameof(Path), IsUnique = true)]
public class DbModelVideoCacheEntry
{
    public int Id { get; set; }
    public string Path { get; set; } = null!;
    public DbModelVideoCacheCoverImage? CoverImage { get; set; }
    public DateTime Date { get; set; }
    public IList<DbModelVideoCaccheEntryTag> Tags { get; } = [];
}

public class DbModelVideoCacheCoverImage
{
    public int Id { get; set; }
    public DbModelVideoCacheEntry VideoCacheEntry { get; } = null!;
    public byte[] ImageData { get; set; } = null!;
}

[Index(nameof(VideoCacheEntryId), nameof(MemberId), IsUnique = true)]
public class DbModelVideoCaccheEntryTag
{
    public int Id { get; set; }
    public int VideoCacheEntryId { get; }
    public DbModelVideoCacheEntry VideoCacheEntry { get; } = null!;
    public int MemberId { get; set; }
    public DbModelGroupMember? Member { get; set; }
    public IList<DbModelVideoCacheEntryTagItem> Items { get; } = [];
}

[Index(nameof(VideoCacheEntryTagId), nameof(CategoryItemId), IsUnique = true)]
public class DbModelVideoCacheEntryTagItem
{
    public int Id { get; set; }
    public int VideoCacheEntryTagId { get; }
    public DbModelVideoCaccheEntryTag VideoCacheEntryTag { get; } = null!;
    public int CategoryItemId { get; set; }
    public DbModelCategoryItem CategoryItem { get; } = null!;
    public bool? BooleanValue { get; set; }
    public int? EnumValueId { get; set; }
    public DbModelCategoryItemEnumValue? EnumValue { get; set; }
}

[Index(nameof(Name), IsUnique = true)]
public class DbModelCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public IList<DbModelCategoryItem> Items { get; } = [];
}

[Index(nameof(CategoryId), nameof(Name), IsUnique = true)]
public class DbModelCategoryItem
{
    public int Id { get; set; }
    public int CategoryId { get; }
    public DbModelCategory Category { get; } = null!;
    public string Name { get; set; } = null!;
    public bool IsBoolean { get; set; }
    public string? BooleanRegex { get; set; }
    public IList<DbModelCategoryItemEnumValue> EnumValues { get; } = [];
}

[Index(nameof(CategoryItemId), nameof(EnumValue), IsUnique = true)]
public class DbModelCategoryItemEnumValue
{
    public int Id { get; set; }
    public int CategoryItemId { get; }
    public DbModelCategoryItem CategoryItem { get; } = null!;
    public string EnumValue { get; set; } = null!;
    public string? Regex { get; set; }
}

[Index(nameof(Name), IsUnique = true)]
public class DbModelGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string[] AlternativeNames { get; set; } = [];
    public IList<DbModelGroupMember> Members { get; } = [];
}

[Index(nameof(GroupId), nameof(Name), IsUnique = true)]
public class DbModelGroupMember
{
    public int Id { get; set; }
    public int GroupId { get; }
    public DbModelGroup Group { get; } = null!;
    public string Name { get; set; } = null!;
    public string[] AlternativeNames { get; set; } = [];
}

public class DbModelFolder
{
    public int Id { get; set; }
    public string Path { get; set; } = null!;
}