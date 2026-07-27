using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NuclearInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reactor_grids",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    total_rows = table.Column<int>(type: "integer", nullable: false),
                    total_columns = table.Column<int>(type: "integer", nullable: false),
                    activity_info_create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activity_info_is_active = table.Column<bool>(type: "boolean", nullable: false),
                    activity_info_update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reactor_grids", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cells",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    x = table.Column<int>(type: "integer", nullable: false),
                    y = table.Column<int>(type: "integer", nullable: false),
                    column_type = table.Column<int>(type: "integer", nullable: false),
                    telemetry = table.Column<string>(type: "jsonb", nullable: false),
                    reactor_grid_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cells", x => x.id);
                    table.ForeignKey(
                        name: "fk_cells_reactor_grids_reactor_grid_id",
                        column: x => x.reactor_grid_id,
                        principalTable: "reactor_grids",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cells_reactor_grid_id",
                table: "cells",
                column: "reactor_grid_id");

            migrationBuilder.CreateIndex(
                name: "ix_reactor_grids_name",
                table: "reactor_grids",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cells");

            migrationBuilder.DropTable(
                name: "reactor_grids");
        }
    }
}
