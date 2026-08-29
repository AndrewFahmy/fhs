using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FHS.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "outbox");

            migrationBuilder.CreateTable(
                name: "actor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_actor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "error_code",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_error_code", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                schema: "outbox",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "station",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_station", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "defect",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    station_id = table.Column<Guid>(type: "uuid", nullable: false),
                    error_code_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolution = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    resolved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_defect", x => x.id);
                    table.ForeignKey(
                        name: "fk_defect_actor_created_by",
                        column: x => x.created_by,
                        principalTable: "actor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_defect_actor_resolved_by",
                        column: x => x.resolved_by,
                        principalTable: "actor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_defect_error_code_error_code_id",
                        column: x => x.error_code_id,
                        principalTable: "error_code",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_defect_station_station_id",
                        column: x => x.station_id,
                        principalTable: "station",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "actor",
                columns: new[] { "id", "display_name", "subject_id" },
                values: new object[,]
                {
                    { new Guid("a5a5a5a5-0000-4000-8000-000000000001"), "Line Operator", "11111111-1111-4111-8111-111111111111" },
                    { new Guid("a5a5a5a5-0000-4000-8000-000000000002"), "System Administrator", "22222222-2222-4222-8222-222222222222" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_actor_subject_id",
                table: "actor",
                column: "subject_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_defect_created_by",
                table: "defect",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_defect_error_code_id",
                table: "defect",
                column: "error_code_id");

            migrationBuilder.CreateIndex(
                name: "ix_defect_resolved_by",
                table: "defect",
                column: "resolved_by");

            migrationBuilder.CreateIndex(
                name: "ix_defect_station_id",
                table: "defect",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "ix_error_code_code",
                table: "error_code",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_messages_occurred_at",
                schema: "outbox",
                table: "messages",
                column: "occurred_at",
                filter: "processed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_station_code",
                table: "station",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "defect");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "outbox");

            migrationBuilder.DropTable(
                name: "actor");

            migrationBuilder.DropTable(
                name: "error_code");

            migrationBuilder.DropTable(
                name: "station");
        }
    }
}
