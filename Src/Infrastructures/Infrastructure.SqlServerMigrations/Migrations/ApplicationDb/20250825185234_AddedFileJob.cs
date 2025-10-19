using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqlServerMigrations.Migrations.ApplicationDb
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    FileJobType = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileJobUploads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskTitle = table.Column<string>(type: "varchar(150)", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AssignedToId = table.Column<int>(type: "int", nullable: true),
                    AssignedTo = table.Column<string>(type: "varchar(60)", nullable: true),
                    Title = table.Column<string>(type: "varchar(150)", nullable: false),
                    Description = table.Column<string>(type: "varchar(350)", nullable: true),
                    FileRowType = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(350)", nullable: true),
                    FileJobId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<byte>(type: "tinyint", nullable: false)
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
                value: new Guid("770175be-5965-49ce-b0f0-b73325ceb678"));

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
                value: new Guid("184c5b11-c859-4f0c-86fe-be2c5b0f0453"));
        }
    }
}
