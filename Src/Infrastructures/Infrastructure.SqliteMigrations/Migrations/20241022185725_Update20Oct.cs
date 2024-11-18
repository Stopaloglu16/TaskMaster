using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqliteMigrations.Migrations
{
    /// <inheritdoc />
    public partial class Update20Oct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "CompletedDate",
                table: "TaskLists",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "TEXT");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AspId", "Created", "CreatedBy", "FullName", "IsDeleted", "LastModified", "LastModifiedBy", "RegisterToken", "RegisterTokenValid", "UserEmail", "UserTypeId" },
                values: new object[] { 1, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "taskmaster@hotmail.co.uk", (byte)0, null, null, new Guid("610dd701-2050-4df8-a13f-adb036242279"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "taskmaster@hotmail.co.uk", 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "CompletedDate",
                table: "TaskLists",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
