using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace stDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderHeadersessionIdParameter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sessionId",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sessionId",
                table: "OrderHeaders");
        }
    }
}
