using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqliteMigrations.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddedFileJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    FileJobType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileJobUploads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskTitle = table.Column<string>(type: "varchar(150)", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    AssignedToId = table.Column<int>(type: "INTEGER", nullable: true),
                    AssignedTo = table.Column<string>(type: "varchar(60)", nullable: true),
                    Title = table.Column<string>(type: "varchar(150)", nullable: false),
                    Description = table.Column<string>(type: "varchar(350)", nullable: true),
                    FileRowType = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(350)", nullable: true),
                    FileJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileJobUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileJobUploads_FileJobs_FileJobId",
                        column: x => x.FileJobId,
                        principalTable: "FileJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegisterToken",
                value: new Guid("53345a4f-1230-43ee-8cf3-cd19792fa6b5"));

            migrationBuilder.CreateIndex(
                name: "IX_FileJobUploads_FileJobId",
                table: "FileJobUploads",
                column: "FileJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileJobUploads");

            migrationBuilder.DropTable(
                name: "FileJobs");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegisterToken",
                value: new Guid("3e5cb58a-1b86-4421-a6d6-32987edce24e"));
        }
    }
}
