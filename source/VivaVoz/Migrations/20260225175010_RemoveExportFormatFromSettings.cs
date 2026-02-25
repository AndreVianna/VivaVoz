using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VivaVoz.Migrations;

/// <inheritdoc />
public partial class RemoveExportFormatFromSettings : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
            name: "ExportFormat",
            table: "Settings");

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<string>(
            name: "ExportFormat",
            table: "Settings",
            type: "TEXT",
            nullable: false,
            defaultValue: "MP3");
}
