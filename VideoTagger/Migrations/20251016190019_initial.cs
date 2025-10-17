using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoTagger.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    AlternativeNames = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Misc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DatabaseVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDirty = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Misc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VideoCacheCoverImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImageData = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoCacheCoverImages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsBoolean = table.Column<bool>(type: "INTEGER", nullable: false),
                    BooleanRegex = table.Column<string>(type: "TEXT", nullable: true),
                    DbModelCategoryId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryItems_Categories_DbModelCategoryId",
                        column: x => x.DbModelCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GroupMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    AlternativeNames = table.Column<string>(type: "TEXT", nullable: false),
                    DbModelGroupId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupMembers_Groups_DbModelGroupId",
                        column: x => x.DbModelGroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VideoCacheEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    CoverImageId = table.Column<int>(type: "INTEGER", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoCacheEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoCacheEntries_VideoCacheCoverImages_CoverImageId",
                        column: x => x.CoverImageId,
                        principalTable: "VideoCacheCoverImages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CategoryItemEnumValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    EnumValue = table.Column<string>(type: "TEXT", nullable: false),
                    Regex = table.Column<string>(type: "TEXT", nullable: true),
                    DbModelCategoryItemId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryItemEnumValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryItemEnumValues_CategoryItems_DbModelCategoryItemId",
                        column: x => x.DbModelCategoryItemId,
                        principalTable: "CategoryItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VideoCacheEntryTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VideoCacheEntryId = table.Column<int>(type: "INTEGER", nullable: false),
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    DbModelVideoCacheEntryId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoCacheEntryTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoCacheEntryTags_GroupMembers_MemberId",
                        column: x => x.MemberId,
                        principalTable: "GroupMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoCacheEntryTags_VideoCacheEntries_DbModelVideoCacheEntryId",
                        column: x => x.DbModelVideoCacheEntryId,
                        principalTable: "VideoCacheEntries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VideoCacheEntryTagItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VideoCacheEntryTagId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    BooleanValue = table.Column<bool>(type: "INTEGER", nullable: true),
                    EnumValueId = table.Column<int>(type: "INTEGER", nullable: true),
                    DbModelVideoCaccheEntryTagId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoCacheEntryTagItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoCacheEntryTagItems_CategoryItemEnumValues_EnumValueId",
                        column: x => x.EnumValueId,
                        principalTable: "CategoryItemEnumValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VideoCacheEntryTagItems_VideoCacheEntryTags_DbModelVideoCaccheEntryTagId",
                        column: x => x.DbModelVideoCaccheEntryTagId,
                        principalTable: "VideoCacheEntryTags",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryItemEnumValues_CategoryItemId_EnumValue",
                table: "CategoryItemEnumValues",
                columns: new[] { "CategoryItemId", "EnumValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryItemEnumValues_DbModelCategoryItemId",
                table: "CategoryItemEnumValues",
                column: "DbModelCategoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryItems_CategoryId_Name",
                table: "CategoryItems",
                columns: new[] { "CategoryId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryItems_DbModelCategoryId",
                table: "CategoryItems",
                column: "DbModelCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_DbModelGroupId",
                table: "GroupMembers",
                column: "DbModelGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_GroupId_Name",
                table: "GroupMembers",
                columns: new[] { "GroupId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Name",
                table: "Groups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoCacheEntries_CoverImageId",
                table: "VideoCacheEntries",
                column: "CoverImageId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCacheEntries_Path",
                table: "VideoCacheEntries",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoCacheEntryTagItems_DbModelVideoCaccheEntryTagId",
                table: "VideoCacheEntryTagItems",
                column: "DbModelVideoCaccheEntryTagId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCacheEntryTagItems_EnumValueId",
                table: "VideoCacheEntryTagItems",
                column: "EnumValueId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCacheEntryTagItems_VideoCacheEntryTagId_CategoryItemId",
                table: "VideoCacheEntryTagItems",
                columns: new[] { "VideoCacheEntryTagId", "CategoryItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoCacheEntryTags_DbModelVideoCacheEntryId",
                table: "VideoCacheEntryTags",
                column: "DbModelVideoCacheEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCacheEntryTags_MemberId",
                table: "VideoCacheEntryTags",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCacheEntryTags_VideoCacheEntryId_MemberId",
                table: "VideoCacheEntryTags",
                columns: new[] { "VideoCacheEntryId", "MemberId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "Misc");

            migrationBuilder.DropTable(
                name: "VideoCacheEntryTagItems");

            migrationBuilder.DropTable(
                name: "CategoryItemEnumValues");

            migrationBuilder.DropTable(
                name: "VideoCacheEntryTags");

            migrationBuilder.DropTable(
                name: "CategoryItems");

            migrationBuilder.DropTable(
                name: "GroupMembers");

            migrationBuilder.DropTable(
                name: "VideoCacheEntries");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "VideoCacheCoverImages");
        }
    }
}
