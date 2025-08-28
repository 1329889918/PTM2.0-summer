using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PTM2._0.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketCS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InitialTicketQuantity",
                table: "Ticket",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitialTicketQuantity",
                table: "Ticket");
        }
    }
}
