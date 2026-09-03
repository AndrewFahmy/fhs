using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FHS.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomersAndEscapes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "escape",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    error_code_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reported_by = table.Column<Guid>(type: "uuid", nullable: false),
                    reported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolution = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    resolved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_escape", x => x.id);
                    table.ForeignKey(
                        name: "fk_escape_actor_reported_by",
                        column: x => x.reported_by,
                        principalTable: "actor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_escape_actor_resolved_by",
                        column: x => x.resolved_by,
                        principalTable: "actor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_escape_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_escape_error_code_error_code_id",
                        column: x => x.error_code_id,
                        principalTable: "error_code",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_customer_code",
                table: "customer",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_escape_customer_id",
                table: "escape",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_escape_error_code_id",
                table: "escape",
                column: "error_code_id");

            migrationBuilder.CreateIndex(
                name: "ix_escape_reported_by",
                table: "escape",
                column: "reported_by");

            migrationBuilder.CreateIndex(
                name: "ix_escape_resolved_by",
                table: "escape",
                column: "resolved_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "escape");

            migrationBuilder.DropTable(
                name: "customer");
        }
    }
}
