using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.PostgresMigrations.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddFileJobSagaAndOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FileJobUploads_FileJobId",
                table: "FileJobUploads");

            migrationBuilder.EnsureSchema(
                name: "messaging");

            // Scaffolded as RenameColumn FileJobType -> Version, which is wrong: the old column was an
            // int enum (0-3) and Version is a concurrency counter that must start at 0. Add the new
            // column and drop the old one instead.
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "FileJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SourceFileJobId",
                table: "TaskLists",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "CompletedDate",
                table: "TaskItems",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<string>(
                name: "BatchKey",
                table: "FileJobUploads",
                type: "varchar(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CorrelationId",
                table: "FileJobs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "FileJobs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "FileJobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "FileJobs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "FileJobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "FileJobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "FileJobs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "NewUpload");

            // Backfill before the unique indexes below are created, or any database with existing
            // rows fails on them: every FileJob would share the all-zero CorrelationId, and every
            // FileJobUpload in a job would share an empty BatchKey.
            migrationBuilder.Sql("""
                UPDATE "FileJobs" SET "CorrelationId" = gen_random_uuid()
                WHERE "CorrelationId" = '00000000-0000-0000-0000-000000000000';
                """);

            // Map the old FileJobType (0 NewUpload, 1 Validated, 2 Running, 3 MovedToLive) onto the
            // saga's states. Running had no saga equivalent — those jobs were mid-flight under the
            // old polling worker — so they restart from NewUpload.
            migrationBuilder.Sql("""
                UPDATE "FileJobs" SET "State" = CASE "FileJobType"
                    WHEN 3 THEN 'Completed'
                    ELSE 'NewUpload'
                END;
                """);

            migrationBuilder.Sql("""
                UPDATE "FileJobUploads" SET "BatchKey" = 'legacy-' || "Id"::text WHERE "BatchKey" = '';
                """);

            migrationBuilder.DropColumn(
                name: "FileJobType",
                table: "FileJobs");

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "messaging",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Consumer = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ReceivedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messages", x => new { x.MessageId, x.Consumer });
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "messaging",
                columns: table => new
                {
                    Sequence = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Sequence);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileJobUploads_FileJobId_BatchKey",
                table: "FileJobUploads",
                columns: new[] { "FileJobId", "BatchKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileJobs_CorrelationId",
                table: "FileJobs",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_Id",
                schema: "messaging",
                table: "outbox_messages",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_pending",
                schema: "messaging",
                table: "outbox_messages",
                column: "Sequence",
                filter: "\"ProcessedOn\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "messaging");

            migrationBuilder.DropIndex(
                name: "IX_FileJobUploads_FileJobId_BatchKey",
                table: "FileJobUploads");

            migrationBuilder.DropIndex(
                name: "IX_FileJobs_CorrelationId",
                table: "FileJobs");

            migrationBuilder.DropColumn(
                name: "SourceFileJobId",
                table: "TaskLists");

            migrationBuilder.DropColumn(
                name: "BatchKey",
                table: "FileJobUploads");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "FileJobs");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "FileJobs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "FileJobs");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "FileJobs");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "FileJobs");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "FileJobs");

            migrationBuilder.DropColumn(
                name: "State",
                table: "FileJobs");

            // Mirror of the Up: drop Version, restore FileJobType rather than renaming one to the
            // other. Everything reverts to NewUpload — the saga's states have no old equivalent.
            migrationBuilder.DropColumn(
                name: "Version",
                table: "FileJobs");

            migrationBuilder.AddColumn<int>(
                name: "FileJobType",
                table: "FileJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "CompletedDate",
                table: "TaskItems",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileJobUploads_FileJobId",
                table: "FileJobUploads",
                column: "FileJobId");
        }
    }
}
