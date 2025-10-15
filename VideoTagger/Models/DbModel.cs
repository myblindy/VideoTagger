using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace VideoTagger.Models;

class DbModel(DbContextOptions<DbModel> options) : DbContext(options)
{
    public DbSet<DbModelVideoCacheEntry> VideoCacheEntries { get; set; } = null!;
    public DbSet<DbModelVideoCaccheEntryTag> VideoCacheEntryTags { get; set; } = null!;
    public DbSet<DbModelVideoCacheEntryTagItem> VideoCacheEntryTagItems { get; set; } = null!;
    public DbSet<DbModelCategory> Categories { get; set; } = null!;
    public DbSet<DbModelCategoryItem> CategoryItems { get; set; } = null!;
    public DbSet<DbModelCategoryItemEnumValue> CategoryItemEnumValues { get; set; } = null!;
    public DbSet<DbModelGroup> Groups { get; set; } = null!;
    public DbSet<DbModelGroupAlternativeName> GroupAlternativeNames { get; set; } = null!;
    public DbSet<DbModelGroupMember> GroupMembers { get; set; } = null!;
    public DbSet<DbModelGroupMemberAlternativeName> GroupMemberAlternativeNames { get; set; } = null!;
    public DbSet<DbModelFolder> Folders { get; set; } = null!;
}

class DbModelVideoCacheEntry
{
    public int Id { get; set; }
    public string Path { get; set; } = null!;
    public string? CoverImageFileName { get; set; }
    public DateTime Date { get; set; }
    public ICollection<DbModelVideoCaccheEntryTag> Tags { get; } = null!;
}

class DbModelVideoCaccheEntryTag
{
    public int Id { get; set; }
    public DbModelVideoCacheEntry VideoCacheEntry { get; } = null!;
    public DbModelGroup? Group { get; set; }
    public DbModelGroupMember? Member { get; set; }
    public ICollection<DbModelVideoCacheEntryTagItem> Items { get; } = null!;
}

class DbModelVideoCacheEntryTagItem
{
    public int Id { get; set; }
    public DbModelVideoCaccheEntryTag VideoCacheEntryTag { get; } = null!;
    public DbModelCategoryItem CategoryItem { get; } = null!;
    public bool? BooleanValue { get; set; }
    public string? EnumValue { get; set; }
}

class DbModelCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<DbModelCategoryItem> Items { get; } = null!;
}

class DbModelCategoryItem
{
    public int Id { get; set; }
    public DbModelCategory Category { get; } = null!;
    public string Name { get; set; } = null!;
    public bool IsBoolean { get; set; }
    public string? BooleanRegex { get; set; }
    public ICollection<DbModelCategoryItemEnumValue> EnumValues { get; } = null!;
}

class DbModelCategoryItemEnumValue
{
    public int Id { get; set; }
    public string EnumValue { get; set; } = null!;
    public string? Regex { get; set; }
}

class DbModelGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<DbModelGroupAlternativeName> AlternativeNames { get; } = null!;
    public ICollection<DbModelGroupMember> Members { get; } = null!;
}

class DbModelGroupAlternativeName
{
    public int Id { get; set; }
    public DbModelGroup Group { get; } = null!;
    public string AlternativeName { get; set; } = null!;
}

class DbModelGroupMember
{
    public int Id { get; set; }
    public DbModelGroup Group { get; } = null!;
    public string Name { get; set; } = null!;
    public ICollection<DbModelGroupMemberAlternativeName> AlternativeNames { get; } = null!;
}

class DbModelGroupMemberAlternativeName
{
    public int Id { get; set; }
    public DbModelGroupMember GroupMember { get; } = null!;
    public string AlternativeName { get; set; } = null!;
}

class DbModelFolder
{
    public int Id { get; set; }
    public string Path { get; set; } = null!;
}