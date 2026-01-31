using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqliteMigrations.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class FixTaskPriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "TaskLists",
                newName: "PriorityId");

            migrationBuilder.AddColumn<int>(
                name: "PriorityId",
                table: "FileJobUploads",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TaskPriority",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskPriority", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskLists_PriorityId",
                table: "TaskLists",
                column: "PriorityId");

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
                name: "TaskPriority");

            migrationBuilder.DropIndex(
                name: "IX_TaskLists_PriorityId",
                table: "TaskLists");

            migrationBuilder.DropColumn(
                name: "PriorityId",
                table: "FileJobUploads");

            migrationBuilder.RenameColumn(
                name: "PriorityId",
                table: "TaskLists",
                newName: "Priority");
        }
    }
}
