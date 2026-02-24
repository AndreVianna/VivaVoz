using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VivaVoz.Migrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class AddSettingsScreenFields : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
        migrationBuilder.AddColumn<bool>(
            name: "RunAtStartup",
            table: "Settings",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "RecordingMode",
            table: "Settings",
            type: "TEXT",
            nullable: false,
            defaultValue: "Toggle");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
        migrationBuilder.DropColumn(name: "RunAtStartup", table: "Settings");
        migrationBuilder.DropColumn(name: "RecordingMode", table: "Settings");
    }
}
