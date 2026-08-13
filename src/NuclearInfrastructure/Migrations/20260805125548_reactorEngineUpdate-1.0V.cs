using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuclearInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class reactorEngineUpdate10V : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_running",
                table: "reactor_grids",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_running",
                table: "reactor_grids");
        }
    }
}
