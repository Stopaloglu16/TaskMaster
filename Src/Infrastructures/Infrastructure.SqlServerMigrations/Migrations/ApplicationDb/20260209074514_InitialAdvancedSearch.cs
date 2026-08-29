using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqlServerMigrations.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class InitialAdvancedSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "search");

            migrationBuilder.CreateTable(
                name: "AdvancedSearches",
                schema: "search",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsDeleted = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedSearches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ColumnTypes",
                schema: "search",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(100)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsDeleted = table.Column<byte>(type: "tinyint", nullable: false)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(100)", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: false),
                    SqlTemplate = table.Column<string>(type: "varchar(150)", nullable: false),
                    ValueMode = table.Column<int>(type: "int", nullable: false),
                    InputControl = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdvancedSearchJoins",
                schema: "search",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdvancedSearchId = table.Column<int>(type: "int", nullable: false),
                    FromTableAlias = table.Column<string>(type: "varchar(50)", nullable: false),
                    ToTableAlias = table.Column<string>(type: "varchar(50)", nullable: false),
                    JoinType = table.Column<string>(type: "varchar(10)", nullable: false),
                    JoinCondition = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsDeleted = table.Column<byte>(type: "tinyint", nullable: false)
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
                name: "AdvancedSearchTables",
                schema: "search",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdvancedSearchId = table.Column<int>(type: "int", nullable: false),
                    TableName = table.Column<string>(type: "varchar(100)", nullable: false),
                    TableAlias = table.Column<string>(type: "varchar(10)", nullable: false),
                    IsBaseTable = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedSearchTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchTables_AdvancedSearches_AdvancedSearchId",
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdvancedSearchId = table.Column<int>(type: "int", nullable: false),
                    TableAlias = table.Column<string>(type: "varchar(50)", nullable: false),
                    ColumnName = table.Column<string>(type: "varchar(100)", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsSelectable = table.Column<bool>(type: "bit", nullable: false),
                    IsFilterable = table.Column<bool>(type: "bit", nullable: false),
                    IsSortable = table.Column<bool>(type: "bit", nullable: false),
                    ColumnTypeId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedSearchColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchColumns_AdvancedSearches_AdvancedSearchId",
                        column: x => x.AdvancedSearchId,
                        principalSchema: "search",
                        principalTable: "AdvancedSearches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdvancedSearchColumns_ColumnTypes_ColumnTypeId",
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
                    ColumnTypesId = table.Column<int>(type: "int", nullable: false),
                    OperatorsId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchColumns_AdvancedSearchId",
                schema: "search",
                table: "AdvancedSearchColumns",
                column: "AdvancedSearchId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchColumns_ColumnTypeId",
                schema: "search",
                table: "AdvancedSearchColumns",
                column: "ColumnTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchJoins_AdvancedSearchId",
                schema: "search",
                table: "AdvancedSearchJoins",
                column: "AdvancedSearchId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedSearchTables_AdvancedSearchId",
                schema: "search",
                table: "AdvancedSearchTables",
                column: "AdvancedSearchId");

            migrationBuilder.CreateIndex(
                name: "IX_ColumnTypeOperatorMapping_OperatorsId",
                schema: "search",
                table: "ColumnTypeOperatorMapping",
                column: "OperatorsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvancedSearchColumns",
                schema: "search");

            migrationBuilder.DropTable(
                name: "AdvancedSearchJoins",
                schema: "search");

            migrationBuilder.DropTable(
                name: "AdvancedSearchTables",
                schema: "search");

            migrationBuilder.DropTable(
                name: "ColumnTypeOperatorMapping",
                schema: "search");

            migrationBuilder.DropTable(
                name: "AdvancedSearches",
                schema: "search");

            migrationBuilder.DropTable(
                name: "ColumnTypes",
                schema: "search");

            migrationBuilder.DropTable(
                name: "Operators",
                schema: "search");
        }
    }
}
