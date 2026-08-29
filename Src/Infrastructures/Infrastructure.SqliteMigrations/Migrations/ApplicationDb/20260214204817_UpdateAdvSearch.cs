using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqliteMigrations.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class UpdateAdvSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvancedSearchColumns_ColumnTypes_ColumnTypeId",
                table: "AdvancedSearchColumns");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvancedSearchTables_AdvancedSearches_AdvancedSearchId",
                table: "AdvancedSearchTables");

            migrationBuilder.DropIndex(
                name: "IX_AdvancedSearchTables_AdvancedSearchId",
                table: "AdvancedSearchTables");

            migrationBuilder.DropIndex(
                name: "IX_AdvancedSearchColumns_ColumnTypeId",
                table: "AdvancedSearchColumns");

            migrationBuilder.DropColumn(
                name: "AdvancedSearchId",
                table: "AdvancedSearchTables");

            migrationBuilder.DropColumn(
                name: "IsBaseTable",
                table: "AdvancedSearchTables");

            migrationBuilder.DropColumn(
                name: "FromTableAlias",
                table: "AdvancedSearchJoins");

            migrationBuilder.DropColumn(
                name: "ToTableAlias",
                table: "AdvancedSearchJoins");

            migrationBuilder.DropColumn(
                name: "ColumnName",
                table: "AdvancedSearchColumns");

            migrationBuilder.DropColumn(
                name: "ColumnTypeId",
                table: "AdvancedSearchColumns");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AdvancedSearchColumns");

            migrationBuilder.DropColumn(
                name: "TableAlias",
                table: "AdvancedSearchColumns");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "AdvancedSearchColumns",
                newName: "ColumnDefinitionId");

            migrationBuilder.AlterColumn<string>(
                name: "TableName",
                table: "AdvancedSearchTables",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.AlterColumn<string>(
                name: "TableAlias",
                table: "AdvancedSearchTables",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AddColumn<string>(
                name: "ForeignKeyColumn",
                table: "AdvancedSearchTables",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentKeyColumn",
                table: "AdvancedSearchTables",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentTableId",
                table: "AdvancedSearchTables",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FromTableAliasId",
                table: "AdvancedSearchJoins",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ToTableAliasId",
                table: "AdvancedSearchJoins",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MainTableId",
                table: "AdvancedSearches",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AdvancedSearchColumnDefinition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TableId = table.Column<int>(type: "INTEGER", nullable: false),
                    ColumnName = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    ColumnTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedSearchColumnDefinition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchColumnDefinition_AdvancedSearchTables_TableId",
                        column: x => x.TableId,
                        principalTable: "AdvancedSearchTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchColumnDefinition_ColumnTypes_ColumnTypeId",
                        column: x => x.ColumnTypeId,
                        principalTable: "ColumnTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdvancedSearchTemplate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdvancedSearchId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    JsonDefinition = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<byte>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedSearchTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchTemplate_AdvancedSearches_AdvancedSearchId",
                        column: x => x.AdvancedSearchId,
                        principalTable: "AdvancedSearches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchTables_ParentTableId",
                table: "AdvancedSearchTables",
                column: "ParentTableId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearches_MainTableId",
                table: "AdvancedSearches",
                column: "MainTableId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchColumns_ColumnDefinitionId",
                table: "AdvancedSearchColumns",
                column: "ColumnDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchColumnDefinition_ColumnTypeId",
                table: "AdvancedSearchColumnDefinition",
                column: "ColumnTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchColumnDefinition_TableId",
                table: "AdvancedSearchColumnDefinition",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchTemplate_AdvancedSearchId",
                table: "AdvancedSearchTemplate",
                column: "AdvancedSearchId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvancedSearchColumns_AdvancedSearchColumnDefinition_ColumnDefinitionId",
                table: "AdvancedSearchColumns",
                column: "ColumnDefinitionId",
                principalTable: "AdvancedSearchColumnDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvancedSearches_AdvancedSearchTables_MainTableId",
                table: "AdvancedSearches",
                column: "MainTableId",
                principalTable: "AdvancedSearchTables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvancedSearchTables_AdvancedSearchTables_ParentTableId",
                table: "AdvancedSearchTables",
                column: "ParentTableId",
                principalTable: "AdvancedSearchTables",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvancedSearchColumns_AdvancedSearchColumnDefinition_ColumnDefinitionId",
                table: "AdvancedSearchColumns");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvancedSearches_AdvancedSearchTables_MainTableId",
                table: "AdvancedSearches");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvancedSearchTables_AdvancedSearchTables_ParentTableId",
                table: "AdvancedSearchTables");

            migrationBuilder.DropTable(
                name: "AdvancedSearchColumnDefinition");

            migrationBuilder.DropTable(
                name: "AdvancedSearchTemplate");

            migrationBuilder.DropIndex(
                name: "IX_AdvancedSearchTables_ParentTableId",
                table: "AdvancedSearchTables");

            migrationBuilder.DropIndex(
                name: "IX_AdvancedSearches_MainTableId",
                table: "AdvancedSearches");

            migrationBuilder.DropIndex(
                name: "IX_AdvancedSearchColumns_ColumnDefinitionId",
                table: "AdvancedSearchColumns");

            migrationBuilder.DropColumn(
                name: "ForeignKeyColumn",
                table: "AdvancedSearchTables");

            migrationBuilder.DropColumn(
                name: "ParentKeyColumn",
                table: "AdvancedSearchTables");

            migrationBuilder.DropColumn(
                name: "ParentTableId",
                table: "AdvancedSearchTables");

            migrationBuilder.DropColumn(
                name: "FromTableAliasId",
                table: "AdvancedSearchJoins");

            migrationBuilder.DropColumn(
                name: "ToTableAliasId",
                table: "AdvancedSearchJoins");

            migrationBuilder.DropColumn(
                name: "MainTableId",
                table: "AdvancedSearches");

            migrationBuilder.RenameColumn(
                name: "ColumnDefinitionId",
                table: "AdvancedSearchColumns",
                newName: "SortOrder");

            migrationBuilder.AlterColumn<string>(
                name: "TableName",
                table: "AdvancedSearchTables",
                type: "varchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "TableAlias",
                table: "AdvancedSearchTables",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "AdvancedSearchId",
                table: "AdvancedSearchTables",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsBaseTable",
                table: "AdvancedSearchTables",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FromTableAlias",
                table: "AdvancedSearchJoins",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToTableAlias",
                table: "AdvancedSearchJoins",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ColumnName",
                table: "AdvancedSearchColumns",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ColumnTypeId",
                table: "AdvancedSearchColumns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AdvancedSearchColumns",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TableAlias",
                table: "AdvancedSearchColumns",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchTables_AdvancedSearchId",
                table: "AdvancedSearchTables",
                column: "AdvancedSearchId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchColumns_ColumnTypeId",
                table: "AdvancedSearchColumns",
                column: "ColumnTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvancedSearchColumns_ColumnTypes_ColumnTypeId",
                table: "AdvancedSearchColumns",
                column: "ColumnTypeId",
                principalTable: "ColumnTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvancedSearchTables_AdvancedSearches_AdvancedSearchId",
                table: "AdvancedSearchTables",
                column: "AdvancedSearchId",
                principalTable: "AdvancedSearches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
