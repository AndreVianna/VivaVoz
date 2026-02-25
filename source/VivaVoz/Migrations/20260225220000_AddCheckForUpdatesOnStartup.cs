using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VivaVoz.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckForUpdatesOnStartup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CheckForUpdatesOnStartup",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckForUpdatesOnStartup",
                table: "Settings");
        }
    }
}
