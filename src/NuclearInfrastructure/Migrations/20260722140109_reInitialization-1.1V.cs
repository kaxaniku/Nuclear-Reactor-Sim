using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NuclearInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class reInitialization11V : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigureCellCommands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false),
                    NewColumnType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigureCellCommands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MoveControlRodCommands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false),
                    TargetInsertionPercentage = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoveControlRodCommands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReactorGrids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TotalRows = table.Column<int>(type: "integer", nullable: false),
                    TotalColumns = table.Column<int>(type: "integer", nullable: false),
                    ActivityInfo_CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActivityInfo_IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ActivityInfo_UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReactorGrids", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReactorOverviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TotalThermalPowerMW = table.Column<double>(type: "double precision", nullable: false),
                    AverageCoolantTemp = table.Column<double>(type: "double precision", nullable: false),
                    ControlRodAverageInsertion = table.Column<double>(type: "double precision", nullable: false),
                    OperatingMargin = table.Column<double>(type: "double precision", nullable: false),
                    IsScrammed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReactorOverviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScramReactorCommands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    IsScrammed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScramReactorCommands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false),
                    ColumnType = table.Column<int>(type: "integer", nullable: false),
                    ReactorGridDtoId = table.Column<int>(type: "integer", nullable: true),
                    Telemetry = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cells_ReactorGrids_ReactorGridDtoId",
                        column: x => x.ReactorGridDtoId,
                        principalTable: "ReactorGrids",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cells_ReactorGridDtoId",
                table: "Cells",
                column: "ReactorGridDtoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cells");

            migrationBuilder.DropTable(
                name: "ConfigureCellCommands");

            migrationBuilder.DropTable(
                name: "MoveControlRodCommands");

            migrationBuilder.DropTable(
                name: "ReactorOverviews");

            migrationBuilder.DropTable(
                name: "ScramReactorCommands");

            migrationBuilder.DropTable(
                name: "ReactorGrids");
        }
    }
}
