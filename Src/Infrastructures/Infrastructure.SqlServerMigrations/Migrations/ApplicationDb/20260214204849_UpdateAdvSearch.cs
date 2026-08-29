using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqlServerMigrations.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class UpdateAdvSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvancedSearchColumns_ColumnTypes_ColumnTypeId",
                schema: "search",
                table: "AdvancedSearchColumns");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvancedSearchTables_AdvancedSearches_AdvancedSearchId",
                schema: "search",
                table: "AdvancedSearchTables");

            migrationBuilder.DropIndex(
                name: "IX_AdvancedSearchTables_AdvancedSearchId",
                schema: "search",
                table: "AdvancedSearchTables");

            migrationBuilder.DropIndex(
                name: "IX_AdvancedSearchColumns_ColumnTypeId",
                schema: "search",
                table: "AdvancedSearchColumns");

            migrationBuilder.DropColumn(
                name: "AdvancedSearchId",
                schema: "search",
                table: "AdvancedSearchTables");

            migrationBuilder.DropColumn(
                name: "IsBaseTable",
                schema: "search",
                table: "AdvancedSearchTables");

            migrationBuilder.DropColumn(
                name: "FromTableAlias",
                schema: "search",
                table: "AdvancedSearchJoins");

            migrationBuilder.DropColumn(
                name: "ToTableAlias",
                schema: "search",
                table: "AdvancedSearchJoins");

            migrationBuilder.DropColumn(
                name: "ColumnName",
                schema: "search",
                table: "AdvancedSearchColumns");

            migrationBuilder.DropColumn(
                name: "ColumnTypeId",
                schema: "search",
                table: "AdvancedSearchColumns");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                schema: "search",
                table: "AdvancedSearchColumns");

            migrationBuilder.DropColumn(
                name: "TableAlias",
                schema: "search",
                table: "AdvancedSearchColumns");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                schema: "search",
                table: "AdvancedSearchColumns",
                newName: "ColumnDefinitionId");

            migrationBuilder.AlterColumn<string>(
                name: "TableName",
                schema: "search",
                table: "AdvancedSearchTables",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.AlterColumn<string>(
                name: "TableAlias",
                schema: "search",
                table: "AdvancedSearchTables",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AddColumn<string>(
                name: "ForeignKeyColumn",
                schema: "search",
                table: "AdvancedSearchTables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentKeyColumn",
                schema: "search",
                table: "AdvancedSearchTables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentTableId",
                schema: "search",
                table: "AdvancedSearchTables",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FromTableAliasId",
                schema: "search",
                table: "AdvancedSearchJoins",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ToTableAliasId",
                schema: "search",
                table: "AdvancedSearchJoins",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MainTableId",
                schema: "search",
                table: "AdvancedSearches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AdvancedSearchColumnDefinition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableId = table.Column<int>(type: "int", nullable: false),
                    ColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColumnTypeId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<byte>(type: "tinyint", nullable: false)
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
                name: "AdvancedSearchTemplate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdvancedSearchId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JsonDefinition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<byte>(type: "tinyint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchTables_ParentTableId",
                schema: "search",
                table: "AdvancedSearchTables",
                column: "ParentTableId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearches_MainTableId",
                schema: "search",
                table: "AdvancedSearches",
                column: "MainTableId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchColumns_ColumnDefinitionId",
                schema: "search",
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
                schema: "search",
                table: "AdvancedSearchColumns",
                column: "ColumnDefinitionId",
                principalTable: "AdvancedSearchColumnDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvancedSearches_AdvancedSearchTables_MainTableId",
                schema: "search",
                table: "AdvancedSearches",
                column: "MainTableId",
                principalSchema: "search",
                principalTable: "AdvancedSearchTables",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvancedSearchTables_AdvancedSearchTables_ParentTableId",
                schema: "search",
                table: "AdvancedSearchTables",
                column: "ParentTableId",
                principalSchema: "search",
                principalTable: "AdvancedSearchTables",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvancedSearchColumns_AdvancedSearchColumnDefinition_ColumnDefinitionId",
                schema: "search",
                table: "AdvancedSearchColumns");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvancedSearches_AdvancedSearchTables_MainTableId",
                schema: "search",
                table: "AdvancedSearches");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvancedSearchTables_AdvancedSearchTables_ParentTableId",
                schema: "search",
                table: "AdvancedSearchTables");

            migrationBuilder.DropTable(
                name: "AdvancedSearchColumnDefinition");

            migrationBuilder.DropTable(
                name: "AdvancedSearchTemplate");

            migrationBuilder.DropIndex(
                name: "IX_AdvancedSearchTables_ParentTableId",
                schema: "search",
                table: "AdvancedSearchTables");

            migrationBuilder.DropIndex(
                name: "IX_AdvancedSearches_MainTableId",
                schema: "search",
                table: "AdvancedSearches");

            migrationBuilder.DropIndex(
                name: "IX_AdvancedSearchColumns_ColumnDefinitionId",
                schema: "search",
                table: "AdvancedSearchColumns");

            migrationBuilder.DropColumn(
                name: "ForeignKeyColumn",
                schema: "search",
                table: "AdvancedSearchTables");

            migrationBuilder.DropColumn(
                name: "ParentKeyColumn",
                schema: "search",
                table: "AdvancedSearchTables");

            migrationBuilder.DropColumn(
                name: "ParentTableId",
                schema: "search",
                table: "AdvancedSearchTables");

            migrationBuilder.DropColumn(
                name: "FromTableAliasId",
                schema: "search",
                table: "AdvancedSearchJoins");

            migrationBuilder.DropColumn(
                name: "ToTableAliasId",
                schema: "search",
                table: "AdvancedSearchJoins");

            migrationBuilder.DropColumn(
                name: "MainTableId",
                schema: "search",
                table: "AdvancedSearches");

            migrationBuilder.RenameColumn(
                name: "ColumnDefinitionId",
                schema: "search",
                table: "AdvancedSearchColumns",
                newName: "SortOrder");

            migrationBuilder.AlterColumn<string>(
                name: "TableName",
                schema: "search",
                table: "AdvancedSearchTables",
                type: "varchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TableAlias",
                schema: "search",
                table: "AdvancedSearchTables",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "AdvancedSearchId",
                schema: "search",
                table: "AdvancedSearchTables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsBaseTable",
                schema: "search",
                table: "AdvancedSearchTables",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FromTableAlias",
                schema: "search",
                table: "AdvancedSearchJoins",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToTableAlias",
                schema: "search",
                table: "AdvancedSearchJoins",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ColumnName",
                schema: "search",
                table: "AdvancedSearchColumns",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ColumnTypeId",
                schema: "search",
                table: "AdvancedSearchColumns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                schema: "search",
                table: "AdvancedSearchColumns",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TableAlias",
                schema: "search",
                table: "AdvancedSearchColumns",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchTables_AdvancedSearchId",
                schema: "search",
                table: "AdvancedSearchTables",
                column: "AdvancedSearchId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchColumns_ColumnTypeId",
                schema: "search",
                table: "AdvancedSearchColumns",
                column: "ColumnTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvancedSearchColumns_ColumnTypes_ColumnTypeId",
                schema: "search",
                table: "AdvancedSearchColumns",
                column: "ColumnTypeId",
                principalSchema: "search",
                principalTable: "ColumnTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvancedSearchTables_AdvancedSearches_AdvancedSearchId",
                schema: "search",
                table: "AdvancedSearchTables",
                column: "AdvancedSearchId",
                principalSchema: "search",
                principalTable: "AdvancedSearches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
