using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PTM2._0.Migrations
{
    /// <inheritdoc />
    public partial class PerformanceStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Performance",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Performance");
        }
    }
}
