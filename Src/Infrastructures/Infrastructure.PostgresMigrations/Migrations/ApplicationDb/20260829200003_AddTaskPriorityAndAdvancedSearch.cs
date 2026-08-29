using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.PostgresMigrations.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddTaskPriorityAndAdvancedSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "search");

            migrationBuilder.AddColumn<int>(
                name: "PriorityId",
                table: "TaskLists",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PriorityId",
                table: "FileJobUploads",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AdvancedSearchTables",
                schema: "search",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    TableAlias = table.Column<string>(type: "text", nullable: false),
                    ParentTableId = table.Column<int>(type: "integer", nullable: true),
                    ForeignKeyColumn = table.Column<string>(type: "text", nullable: true),
                    ParentKeyColumn = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedSearchTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchTables_AdvancedSearchTables_ParentTableId",
                        column: x => x.ParentTableId,
                        principalSchema: "search",
                        principalTable: "AdvancedSearchTables",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ColumnTypes",
                schema: "search",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "varchar(100)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsDeleted = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColumnTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                schema: "search",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "varchar(100)", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: false),
                    SqlTemplate = table.Column<string>(type: "varchar(150)", nullable: false),
                    ValueMode = table.Column<int>(type: "integer", nullable: false),
                    InputControl = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskPriority",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false),
                    Color = table.Column<string>(type: "varchar(50)", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskPriority", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdvancedSearches",
                schema: "search",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    MainTableId = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedSearches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedSearches_AdvancedSearchTables_MainTableId",
                        column: x => x.MainTableId,
                        principalSchema: "search",
                        principalTable: "AdvancedSearchTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdvancedSearchColumnDefinition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableId = table.Column<int>(type: "integer", nullable: false),
                    ColumnName = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    ColumnTypeId = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedSearchColumnDefinition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchColumnDefinition_AdvancedSearchTables_TableId",
                        column: x => x.TableId,
                        principalSchema: "search",
                        principalTable: "AdvancedSearchTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchColumnDefinition_ColumnTypes_ColumnTypeId",
                        column: x => x.ColumnTypeId,
                        principalSchema: "search",
                        principalTable: "ColumnTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ColumnTypeOperatorMapping",
                schema: "search",
                columns: table => new
                {
                    ColumnTypesId = table.Column<int>(type: "integer", nullable: false),
                    OperatorsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColumnTypeOperatorMapping", x => new { x.ColumnTypesId, x.OperatorsId });
                    table.ForeignKey(
                        name: "FK_ColumnTypeOperatorMapping_ColumnTypes_ColumnTypesId",
                        column: x => x.ColumnTypesId,
                        principalSchema: "search",
                        principalTable: "ColumnTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ColumnTypeOperatorMapping_Operators_OperatorsId",
                        column: x => x.OperatorsId,
                        principalSchema: "search",
                        principalTable: "Operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdvancedSearchJoins",
                schema: "search",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdvancedSearchId = table.Column<int>(type: "integer", nullable: false),
                    FromTableAliasId = table.Column<int>(type: "integer", nullable: false),
                    ToTableAliasId = table.Column<int>(type: "integer", nullable: false),
                    JoinType = table.Column<string>(type: "varchar(10)", nullable: false),
                    JoinCondition = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsDeleted = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedSearchJoins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchJoins_AdvancedSearches_AdvancedSearchId",
                        column: x => x.AdvancedSearchId,
                        principalSchema: "search",
                        principalTable: "AdvancedSearches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdvancedSearchTemplate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdvancedSearchId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    JsonDefinition = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<byte>(type: "smallint", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedSearchTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchTemplate_AdvancedSearches_AdvancedSearchId",
                        column: x => x.AdvancedSearchId,
                        principalSchema: "search",
                        principalTable: "AdvancedSearches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdvancedSearchColumns",
                schema: "search",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdvancedSearchId = table.Column<int>(type: "integer", nullable: false),
                    ColumnDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    IsSelectable = table.Column<bool>(type: "boolean", nullable: false),
                    IsFilterable = table.Column<bool>(type: "boolean", nullable: false),
                    IsSortable = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedSearchColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchColumns_AdvancedSearchColumnDefinition_Column~",
                        column: x => x.ColumnDefinitionId,
                        principalTable: "AdvancedSearchColumnDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchColumns_AdvancedSearches_AdvancedSearchId",
                        column: x => x.AdvancedSearchId,
                        principalSchema: "search",
                        principalTable: "AdvancedSearches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskLists_PriorityId",
                table: "TaskLists",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchColumnDefinition_ColumnTypeId",
                table: "AdvancedSearchColumnDefinition",
                column: "ColumnTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchColumnDefinition_TableId",
                table: "AdvancedSearchColumnDefinition",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchColumns_AdvancedSearchId",
                schema: "search",
                table: "AdvancedSearchColumns",
                column: "AdvancedSearchId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchColumns_ColumnDefinitionId",
                schema: "search",
                table: "AdvancedSearchColumns",
                column: "ColumnDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearches_MainTableId",
                schema: "search",
                table: "AdvancedSearches",
                column: "MainTableId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchJoins_AdvancedSearchId",
                schema: "search",
                table: "AdvancedSearchJoins",
                column: "AdvancedSearchId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchTables_ParentTableId",
                schema: "search",
                table: "AdvancedSearchTables",
                column: "ParentTableId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchTemplate_AdvancedSearchId",
                table: "AdvancedSearchTemplate",
                column: "AdvancedSearchId");

            migrationBuilder.CreateIndex(
                name: "IX_ColumnTypeOperatorMapping_OperatorsId",
                schema: "search",
                table: "ColumnTypeOperatorMapping",
                column: "OperatorsId");

            // TaskLists.PriorityId was added above with defaultValue 0, and the foreign key below
            // points at TaskPriority.Id — so on any database that already has task lists this fails
            // with 23503 unless the rows are given a real priority first. Nothing seeds TaskPriority
            // anywhere in the app, so seed the three the entity documents and backfill.
            migrationBuilder.Sql("""
                INSERT INTO "TaskPriority" ("Name","Color","SortOrder","IsDefault","IsDeleted")
                SELECT * FROM (VALUES
                    ('Low',    'secondary', 1, false, CAST(0 AS smallint)),
                    ('Medium', 'warning',   2, true,  CAST(0 AS smallint)),
                    ('High',   'danger',    3, false, CAST(0 AS smallint))
                ) AS seed(name, color, sortorder, isdefault, isdeleted)
                WHERE NOT EXISTS (SELECT 1 FROM "TaskPriority");
                """);

            migrationBuilder.Sql("""
                UPDATE "TaskLists"
                SET "PriorityId" = COALESCE(
                    (SELECT MIN("Id") FROM "TaskPriority" WHERE "IsDefault"),
                    (SELECT MIN("Id") FROM "TaskPriority"))
                WHERE "PriorityId" = 0 AND EXISTS (SELECT 1 FROM "TaskPriority");
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskLists_TaskPriority_PriorityId",
                table: "TaskLists",
                column: "PriorityId",
                principalTable: "TaskPriority",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskLists_TaskPriority_PriorityId",
                table: "TaskLists");

            migrationBuilder.DropTable(
                name: "AdvancedSearchColumns",
                schema: "search");

            migrationBuilder.DropTable(
                name: "AdvancedSearchJoins",
                schema: "search");

            migrationBuilder.DropTable(
                name: "AdvancedSearchTemplate");

            migrationBuilder.DropTable(
                name: "ColumnTypeOperatorMapping",
                schema: "search");

            migrationBuilder.DropTable(
                name: "TaskPriority");

            migrationBuilder.DropTable(
                name: "AdvancedSearchColumnDefinition");

            migrationBuilder.DropTable(
                name: "AdvancedSearches",
                schema: "search");

            migrationBuilder.DropTable(
                name: "Operators",
                schema: "search");

            migrationBuilder.DropTable(
                name: "ColumnTypes",
                schema: "search");

            migrationBuilder.DropTable(
                name: "AdvancedSearchTables",
                schema: "search");

            migrationBuilder.DropIndex(
                name: "IX_TaskLists_PriorityId",
                table: "TaskLists");

            migrationBuilder.DropColumn(
                name: "PriorityId",
                table: "TaskLists");

            migrationBuilder.DropColumn(
                name: "PriorityId",
                table: "FileJobUploads");
        }
    }
}
