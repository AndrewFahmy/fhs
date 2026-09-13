using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

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
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "error_codes",
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
                    table.PrimaryKey("pk_error_codes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "facilities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_facilities", x => x.id);
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
                name: "actors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_actors", x => x.id);
                    table.ForeignKey(
                        name: "fk_actors_facility_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stations", x => x.id);
                    table.ForeignKey(
                        name: "fk_stations_facilities_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "defects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    station_id = table.Column<Guid>(type: "uuid", nullable: false),
                    error_code_id = table.Column<Guid>(type: "uuid", nullable: false),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("pk_defects", x => x.id);
                    table.ForeignKey(
                        name: "fk_defects_actors_created_by",
                        column: x => x.created_by,
                        principalTable: "actors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_defects_actors_resolved_by",
                        column: x => x.resolved_by,
                        principalTable: "actors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_defects_error_code_error_code_id",
                        column: x => x.error_code_id,
                        principalTable: "error_codes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_defects_facility_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_defects_station_station_id",
                        column: x => x.station_id,
                        principalTable: "stations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "escapes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    error_code_id = table.Column<Guid>(type: "uuid", nullable: false),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reported_by = table.Column<Guid>(type: "uuid", nullable: false),
                    reported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolution = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    resolved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    attributed_defect_id = table.Column<Guid>(type: "uuid", nullable: true),
                    attribution_basis = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)                    
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_escapes", x => x.id);
                    table.CheckConstraint("ck_escapes_attribution_complete", "(attributed_defect_id IS NULL) = (attribution_basis IS NULL)");
                    table.ForeignKey(
                        name: "fk_escapes_actors_reported_by",
                        column: x => x.reported_by,
                        principalTable: "actors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_escapes_actors_resolved_by",
                        column: x => x.resolved_by,
                        principalTable: "actors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_escapes_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_escapes_defects_attributed_defect_id",
                        column: x => x.attributed_defect_id,
                        principalTable: "defects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_escapes_error_codes_error_code_id",
                        column: x => x.error_code_id,
                        principalTable: "error_codes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_escapes_facility_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "actors",
                columns: new[] { "id", "display_name", "facility_id", "kind", "subject_id" },
                values: new object[] { new Guid("a5a5a5a5-0000-4000-8000-000000000001"), "System Administrator", null, "Person", "11111111-1111-4111-8111-111111111111" });

            migrationBuilder.CreateIndex(
                name: "ix_actors_facility_id",
                table: "actors",
                column: "facility_id");

            migrationBuilder.CreateIndex(
                name: "ix_actors_subject_id",
                table: "actors",
                column: "subject_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customers_code",
                table: "customers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_defects_created_by",
                table: "defects",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_defects_error_code_id",
                table: "defects",
                column: "error_code_id");

            migrationBuilder.CreateIndex(
                name: "ix_defects_facility_id_created_at",
                table: "defects",
                columns: new[] { "facility_id", "created_at" },
                descending: new[] { false, true },
                filter: "resolved_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_defects_facility_id_created_at_id",
                table: "defects",
                columns: new[] { "facility_id", "created_at", "id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "ix_defects_facility_id_error_code_id_created_at",
                table: "defects",
                columns: new[] { "facility_id", "error_code_id", "created_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_defects_facility_id_station_id_created_at",
                table: "defects",
                columns: new[] { "facility_id", "station_id", "created_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_defects_resolved_by",
                table: "defects",
                column: "resolved_by");

            migrationBuilder.CreateIndex(
                name: "ix_defects_station_id",
                table: "defects",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "ix_error_codes_code",
                table: "error_codes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_escapes_attributed_defect_id",
                table: "escapes",
                column: "attributed_defect_id",
                filter: "attributed_defect_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_escapes_customer_id",
                table: "escapes",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_escapes_error_code_id",
                table: "escapes",
                column: "error_code_id");

            migrationBuilder.CreateIndex(
                name: "ix_escapes_facility_id_customer_id_reported_at",
                table: "escapes",
                columns: new[] { "facility_id", "customer_id", "reported_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_escapes_facility_id_error_code_id_reported_at",
                table: "escapes",
                columns: new[] { "facility_id", "error_code_id", "reported_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_escapes_facility_id_reported_at",
                table: "escapes",
                columns: new[] { "facility_id", "reported_at" },
                descending: new[] { false, true },
                filter: "resolved_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_escapes_facility_id_reported_at_id",
                table: "escapes",
                columns: new[] { "facility_id", "reported_at", "id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "ix_escapes_reported_by",
                table: "escapes",
                column: "reported_by");

            migrationBuilder.CreateIndex(
                name: "ix_escapes_resolved_by",
                table: "escapes",
                column: "resolved_by");

            migrationBuilder.CreateIndex(
                name: "ix_facilities_code",
                table: "facilities",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_messages_occurred_at",
                schema: "outbox",
                table: "messages",
                column: "occurred_at",
                filter: "processed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_stations_code",
                table: "stations",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stations_facility_id",
                table: "stations",
                column: "facility_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "escapes");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "outbox");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "defects");

            migrationBuilder.DropTable(
                name: "actors");

            migrationBuilder.DropTable(
                name: "error_codes");

            migrationBuilder.DropTable(
                name: "stations");

            migrationBuilder.DropTable(
                name: "facilities");
        }
    }
}
