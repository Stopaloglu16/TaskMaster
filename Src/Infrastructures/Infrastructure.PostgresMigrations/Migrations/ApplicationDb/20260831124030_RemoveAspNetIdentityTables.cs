using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.PostgresMigrations.Migrations.ApplicationDb
{
    /// <summary>
    /// Drops the seven ASP.NET Identity tables left behind by the Keycloak cut-over, along with the
    /// history row for the WebIdentityContext migration that created them.
    ///
    /// Raw SQL rather than DropTable: these tables were never part of ApplicationDbContext's model,
    /// so the scaffolded Up/Down came out empty and DropTable would desync the snapshot. Both
    /// contexts shared public.__EFMigrationsHistory, which is why the orphan row has to go too —
    /// WebIdentityContext no longer exists to own it.
    /// </summary>
    public partial class RemoveAspNetIdentityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CASCADE covers the FKs between them; IF EXISTS keeps this a no-op on a database
            // created after the Identity migration was deleted.
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS "AspNetRoleClaims",
                                     "AspNetUserClaims",
                                     "AspNetUserLogins",
                                     "AspNetUserRoles",
                                     "AspNetUserTokens",
                                     "AspNetUsers",
                                     "AspNetRoles" CASCADE;

                DELETE FROM "__EFMigrationsHistory"
                WHERE "MigrationId" = '20260829174259_InitialWebIdCreate';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Deliberately empty. Recreating the Identity schema would mean restoring
            // WebIdentityContext, which no longer exists; Keycloak owns credentials now.
        }
    }
}
