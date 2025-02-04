using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqliteMigrations.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class GuidUserNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "RegisterToken", "UserGuidId" },
                values: new object[] { new Guid("07fc6358-3978-4c38-8747-535a8faf3319"), new Guid("11e998be-9d1b-4c6b-acd6-a75752a2c47c") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "RegisterToken", "UserGuidId" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000000"), new Guid("00000000-0000-0000-0000-000000000000") });
        }
    }
}
