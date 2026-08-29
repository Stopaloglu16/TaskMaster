using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqliteMigrations.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class InitialSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdvancedSearches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsDeleted = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedSearches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ColumnTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "varchar(100)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsDeleted = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColumnTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "varchar(100)", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: false),
                    SqlTemplate = table.Column<string>(type: "varchar(150)", nullable: false),
                    ValueMode = table.Column<int>(type: "INTEGER", nullable: false),
                    InputControl = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdvancedSearchJoins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdvancedSearchId = table.Column<int>(type: "INTEGER", nullable: false),
                    FromTableAlias = table.Column<string>(type: "varchar(50)", nullable: false),
                    ToTableAlias = table.Column<string>(type: "varchar(50)", nullable: false),
                    JoinType = table.Column<string>(type: "varchar(10)", nullable: false),
                    JoinCondition = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsDeleted = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedSearchJoins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchJoins_AdvancedSearches_AdvancedSearchId",
                        column: x => x.AdvancedSearchId,
                        principalTable: "AdvancedSearches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdvancedSearchTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdvancedSearchId = table.Column<int>(type: "INTEGER", nullable: false),
                    TableName = table.Column<string>(type: "varchar(100)", nullable: false),
                    TableAlias = table.Column<string>(type: "varchar(10)", nullable: false),
                    IsBaseTable = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedSearchTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchTables_AdvancedSearches_AdvancedSearchId",
                        column: x => x.AdvancedSearchId,
                        principalTable: "AdvancedSearches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdvancedSearchColumns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdvancedSearchId = table.Column<int>(type: "INTEGER", nullable: false),
                    TableAlias = table.Column<string>(type: "varchar(50)", nullable: false),
                    ColumnName = table.Column<string>(type: "varchar(100)", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSelectable = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsFilterable = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSortable = table.Column<bool>(type: "INTEGER", nullable: false),
                    ColumnTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedSearchColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchColumns_AdvancedSearches_AdvancedSearchId",
                        column: x => x.AdvancedSearchId,
                        principalTable: "AdvancedSearches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchColumns_ColumnTypes_ColumnTypeId",
                        column: x => x.ColumnTypeId,
                        principalTable: "ColumnTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ColumnTypeOperatorMapping",
                columns: table => new
                {
                    ColumnTypesId = table.Column<int>(type: "INTEGER", nullable: false),
                    OperatorsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColumnTypeOperatorMapping", x => new { x.ColumnTypesId, x.OperatorsId });
                    table.ForeignKey(
                        name: "FK_ColumnTypeOperatorMapping_ColumnTypes_ColumnTypesId",
                        column: x => x.ColumnTypesId,
                        principalTable: "ColumnTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ColumnTypeOperatorMapping_Operators_OperatorsId",
                        column: x => x.OperatorsId,
                        principalTable: "Operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchColumns_AdvancedSearchId",
                table: "AdvancedSearchColumns",
                column: "AdvancedSearchId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchColumns_ColumnTypeId",
                table: "AdvancedSearchColumns",
                column: "ColumnTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchJoins_AdvancedSearchId",
                table: "AdvancedSearchJoins",
                column: "AdvancedSearchId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchTables_AdvancedSearchId",
                table: "AdvancedSearchTables",
                column: "AdvancedSearchId");

            migrationBuilder.CreateIndex(
                name: "IX_ColumnTypeOperatorMapping_OperatorsId",
                table: "ColumnTypeOperatorMapping",
                column: "OperatorsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvancedSearchColumns");

            migrationBuilder.DropTable(
                name: "AdvancedSearchJoins");

            migrationBuilder.DropTable(
                name: "AdvancedSearchTables");

            migrationBuilder.DropTable(
                name: "ColumnTypeOperatorMapping");

            migrationBuilder.DropTable(
                name: "AdvancedSearches");

            migrationBuilder.DropTable(
                name: "ColumnTypes");

            migrationBuilder.DropTable(
                name: "Operators");
        }
    }
}
